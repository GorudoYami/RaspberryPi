using GorudoYami.Common.Cryptography;
using GorudoYami.Common.Modules;
using GorudoYami.Common.Streams;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Exceptions;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Utilities;
using RaspberryPi.Modem.Exceptions;
using RaspberryPi.Modem.Models;
using System.IO.Ports;
using System.Net;
using System.Security.Cryptography;
using System.Threading;

namespace RaspberryPi.Modem;

public class ModemModule : IModemModule, IDisposable {
	public bool IsInitialized { get; private set; }

	private readonly ILogger<IModemModule> _logger;
	private readonly SerialPort _serialPort;
	private readonly int _targetBaudRate;
	private readonly int _defaultBaudRate;
	private readonly IPAddress _serverAddress;
	private readonly int _serverPort;

	private ByteStreamReader? _serverUnencryptedReader;
	private CryptoStreamReaderWriter? _serverReaderWriter;
	private Aes? _serverAes;

	// Need to create SerialPortProvider for unit testing
	public ModemModule(IOptions<ModemModuleOptions> options, ILogger<IModemModule> logger) {
		_logger = logger;
		_targetBaudRate = options.Value.TargetBaudRate;
		_defaultBaudRate = options.Value.DefaultBaudRate;
		_serverAddress = Networking.GetAddressFromHostname(options.Value.ServerHost);
		_serverPort = options.Value.ServerPort;
		_serialPort = new SerialPort(options.Value.SerialPort) {
			BaudRate = _targetBaudRate,
			DataBits = 8,
			StopBits = StopBits.One,
			Parity = Parity.None,
			Handshake = Handshake.RequestToSend,
			ReadTimeout = options.Value.DefaultTimeoutSeconds * 1000,
			WriteTimeout = options.Value.DefaultTimeoutSeconds * 1000,
			NewLine = "\r\n",
		};
	}

	public bool SendCommand(string command, string expectedResponse = "OK", bool throwOnFail = false) {
		string? response = null;

		try {
			WriteLine(command);
			response = ReadLine();
		}
		catch (Exception ex) {
			if (throwOnFail) {
				throw new SendCommandException($"Command {command} failed with response {response ?? "<null>"}", ex);
			}
			else {
				_logger.LogError(ex, "Command {Command} failed with response {Response}", command, response ?? "<null>");
			}
		}

		bool containsExpectedResponse = response?.Contains(expectedResponse) ?? false;

		if (throwOnFail && containsExpectedResponse == false) {
			throw new SendCommandException($"Command {command} failed with response {response ?? "<null>"}");
		}

		return containsExpectedResponse;
	}

	private void WriteLine(string message) {
		while (_serialPort.CtsHolding == false) {
			Thread.Sleep(5);
		}

		_logger.LogDebug("[Sent] {Message}", message);
		_serialPort.WriteLine(message);
	}

	private string ReadLine(bool skipEcho = true) {
		string message = _serialPort.ReadLine();
		if (skipEcho) {
			_logger.LogDebug("[Received] [Echo] {Response}", message);
			message = _serialPort.ReadLine();
		}

		_logger.LogDebug("[Received] {Response}", message);
		return message;
	}

	public Task InitializeAsync(CancellationToken cancellationToken = default) {
		return Task.Run(() => {
			InitializeBaudRate();
			SendCommand("AT+IFC=2,2", throwOnFail: true);
			SendCommand("AT+CFUN=7", throwOnFail: true);
			IsInitialized = true;
		}, cancellationToken);
	}

	private void InitializeBaudRate() {
		_serialPort.Open();
		_serialPort.DiscardInBuffer();
		_serialPort.DiscardOutBuffer();

		if (SendCommand("AT")) {
			return;
		}

		_serialPort.Close();
		_serialPort.BaudRate = _defaultBaudRate;
		_serialPort.Open();
		_serialPort.DiscardInBuffer();
		_serialPort.DiscardOutBuffer();

		if (SendCommand("AT") == false) {
			throw new InitializeModuleException($"Could not initialize communication with baud rates {_targetBaudRate} and {_defaultBaudRate}");
		}

		if (SendCommand($"AT+IPR={_targetBaudRate}") == false) {
			throw new InitializeModuleException($"Could not set target baud rate {_targetBaudRate}");
		}

		_serialPort.Close();
		_serialPort.BaudRate = _targetBaudRate;
		_serialPort.Open();
		_serialPort.DiscardInBuffer();
		_serialPort.DiscardOutBuffer();

		if (SendCommand("AT")) {
			throw new InitializeModuleException($"Could not communicate at target baud rate {_targetBaudRate}");
		}
	}

	public async Task StartAsync(CancellationToken cancellationToken = default) {
		SendCommand("AT+CFUN=1", throwOnFail: true);
		SendCommand("AT+COPS=0", throwOnFail: true);
		SendCommand("AT+CEREG=1", throwOnFail: true);
		SendCommand("AT+CGACT=1", throwOnFail: true);
		SendCommand("AT+CIPMODE=1", throwOnFail: true);
		SendCommand("AT+CSTT=\"internet\"", throwOnFail: true);
		SendCommand("AT+CIICR", throwOnFail: true);
		SendCommand("AT+CIFSR", throwOnFail: true);
		SendCommand($"AT+CIPSTART=\"TCP\",\"{_serverAddress}\",\"{_serverPort}\"", throwOnFail: true);

		await InitializeCommunicationAsync(cancellationToken);
	}

	private async Task InitializeCommunicationAsync(CancellationToken cancellationToken) {
		Stream serverStream = _serialPort.BaseStream;
		_serverUnencryptedReader = new ByteStreamReader(_serialPort.BaseStream, true);

		using var rsa = RSA.Create(CryptographyKeySizes.RsaKeySizeBits);
		await serverStream.WriteAsync(rsa.ExportRSAPublicKey(), cancellationToken);

		bool result = false;
		try {
			_serverAes = Aes.Create();
			_serverAes.KeySize = CryptographyKeySizes.AesKeySizeBits;

			byte[] data = await _serverUnencryptedReader.ReadMessageAsync(cancellationToken);
			int expectedLength = CryptographyKeySizes.AesKeySizeBits / 8;
			if (data.Length != expectedLength) {
				throw new InitializeCommunicationException($"Received AES key has an invalid size. Expected {expectedLength}. Actual: {data.Length}");
			}
			_serverAes.Key = rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA512);

			data = await _serverUnencryptedReader.ReadMessageAsync(cancellationToken);
			expectedLength = CryptographyKeySizes.AesIvSizeBits / 8;
			if (data.Length != expectedLength) {
				throw new InitializeCommunicationException($"Received AES IV has an invalid size. Expected {expectedLength}. Actual: {data.Length}");
			}
			_serverAes.IV = rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA512);

			_serverReaderWriter = new CryptoStreamReaderWriter(_serverAes.CreateEncryptor(), _serverAes.CreateDecryptor(), serverStream);
			await _serverReaderWriter.WriteLineAsync("OK", cancellationToken);

			result = true;
		}
		finally {
			if (result == false) {
				//Disconnect();
			}
		}
	}

	public void Dispose() {
		GC.SuppressFinalize(this);
		_serialPort.Close();
		_serialPort.Dispose();
	}
}

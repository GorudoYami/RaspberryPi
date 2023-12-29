using GorudoYami.Common.Cryptography;
using GorudoYami.Common.Modules;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Common.Utilities;
using RaspberryPi.Modem.Exceptions;
using RaspberryPi.Modem.Models;
using System.IO.Ports;
using System.Net;

namespace RaspberryPi.Modem;

public class ModemModule : IModemModule, IDisposable {
	public bool LazyInitialization => false;
	public bool IsInitialized { get; private set; }
	public IPAddress ServerAddress => Networking.GetAddressFromHostname(_options.ServerHost);
	public bool Connected => _serverReaderWriter != null;

	private readonly ILogger<IModemModule> _logger;
	private readonly SerialPort _serialPort;
	private readonly ModemModuleOptions _options;
	private readonly IProtocol _protocol;

	private CryptoStreamReaderWriter? _serverReaderWriter;

	public ModemModule(IOptions<ModemModuleOptions> options, ILogger<IModemModule> logger, IClientProtocol protocol) {
		_logger = logger;
		_options = options.Value;
		_protocol = protocol;
		_serialPort = new SerialPort(_options.SerialPort) {
			BaudRate = _options.TargetBaudRate,
			DataBits = 8,
			StopBits = StopBits.One,
			Parity = Parity.None,
			Handshake = Handshake.RequestToSend,
			ReadTimeout = _options.TimeoutSeconds * 1000,
			WriteTimeout = _options.TimeoutSeconds * 1000,
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
		_serialPort.BaudRate = _options.DefaultBaudRate;
		_serialPort.Open();
		_serialPort.DiscardInBuffer();
		_serialPort.DiscardOutBuffer();

		if (SendCommand("AT") == false) {
			throw new InitializeModuleException($"Could not initialize communication with baud rates {_options.TargetBaudRate} and {_options.DefaultBaudRate}");
		}

		if (SendCommand($"AT+IPR={_options.TargetBaudRate}") == false) {
			throw new InitializeModuleException($"Could not set target baud rate {_options.TargetBaudRate}");
		}

		_serialPort.Close();
		_serialPort.BaudRate = _options.TargetBaudRate;
		_serialPort.Open();
		_serialPort.DiscardInBuffer();
		_serialPort.DiscardOutBuffer();

		if (SendCommand("AT")) {
			throw new InitializeModuleException($"Could not communicate at target baud rate {_options.TargetBaudRate}");
		}
	}

	public async Task ConnectAsync(CancellationToken cancellationToken = default) {
		SendCommand("AT+CFUN=1", throwOnFail: true);
		SendCommand("AT+COPS=0", throwOnFail: true);
		SendCommand("AT+CEREG=1", throwOnFail: true);
		SendCommand("AT+CGACT=1", throwOnFail: true);
		SendCommand("AT+CIPMODE=1", throwOnFail: true);
		SendCommand("AT+CSTT=\"internet\"", throwOnFail: true);
		SendCommand("AT+CIICR", throwOnFail: true);
		SendCommand("AT+CIFSR", throwOnFail: true);
		SendCommand($"AT+CIPSTART=\"TCP\",\"{ServerAddress}\",\"{_options.ServerPort}\"", throwOnFail: true);

		_serverReaderWriter = await _protocol.InitializeCommunicationAsync(_serialPort.BaseStream, cancellationToken) as CryptoStreamReaderWriter
			?? throw new InvalidOperationException("Protocol returned stream of wrong type");
	}

	public async Task DisconnectAsync(CancellationToken cancellationToken = default) {
		_serialPort.DtrEnable = false;
		await Task.Delay(1000, cancellationToken);
		_serialPort.DtrEnable = true;
		await Task.Delay(25, cancellationToken);

		SendCommand("AT+CIPCLOSE", throwOnFail: true);
		SendCommand("AT+CIPSHUT", throwOnFail: true);
		SendCommand("AT+CFUN=7", throwOnFail: true);
	}

	public void Dispose() {
		GC.SuppressFinalize(this);
		_serialPort.Close();
		_serialPort.Dispose();
	}
}

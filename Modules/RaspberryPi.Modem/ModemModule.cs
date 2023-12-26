using GorudoYami.Common.Modules;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Utilities;
using RaspberryPi.Modem.Exceptions;
using RaspberryPi.Modem.Models;
using System.IO.Ports;
using System.Net;

namespace RaspberryPi.Modem;

public class ModemModule : IModemModule, IDisposable {
	public bool IsInitialized { get; private set; }

	private readonly ILogger<IModemModule> _logger;
	private readonly SerialPort _serialPort;
	private readonly int _targetBaudRate;
	private readonly int _defaultBaudRate;
	private readonly IPAddress _serverAddress;
	private readonly int _serverPort;

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
		};
	}

	public bool SendCommand(string command, string expectedResponse = "OK", bool throwOnFail = false) {
		WriteLine(command);
		string response = ReadLine();
		bool receivedExpectedResponse = response.Contains(expectedResponse);

		if (throwOnFail && receivedExpectedResponse == false) {
			throw new SendCommandException($"Command {command} failed with response {response}");
		}

		return receivedExpectedResponse;
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
			SendCommand("AT+CFUN=7", throwOnFail: true);
			IsInitialized = true;
		}, cancellationToken);
	}

	private void InitializeBaudRate() {
		_serialPort.Open();
		if (SendCommand("AT")) {
			return;
		}

		_serialPort.Close();
		_serialPort.BaudRate = _defaultBaudRate;
		_serialPort.Open();

		if (SendCommand("AT") == false) {
			throw new InitializeModuleException($"Could not initialize communication with baud rates {_targetBaudRate} and {_defaultBaudRate}");
		}

		if (SendCommand($"AT+IPR={_targetBaudRate}") == false) {
			throw new InitializeModuleException($"Could not set target baud rate {_targetBaudRate}");
		}

		_serialPort.Close();
		_serialPort.BaudRate = _targetBaudRate;
		_serialPort.Open();

		if (SendCommand("AT")) {
			throw new InitializeModuleException($"Could not communicate at target baud rate {_targetBaudRate}");
		}
	}

	public void Start() {
		SendCommand("AT+CFUN=1", throwOnFail: true);
		SendCommand("AT+COPS=0", throwOnFail: true);
		SendCommand("AT+CEREG=1", throwOnFail: true);
		SendCommand("AT+CGACT=1", throwOnFail: true);
		SendCommand($"AT+QIOPEN=\"TCP\",\"{_serverAddress}\",\"{_serverPort}\"", throwOnFail: true);
	}

	public void Dispose() {
		GC.SuppressFinalize(this);
		_serialPort.Close();
		_serialPort.Dispose();
	}
}

using GorudoYami.Common.Cryptography;
using GorudoYami.Common.Modules;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Modem.Exceptions;
using RaspberryPi.Modem.Options;
using RaspberryPi.Modem.Validators;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Modem {
	public class ModemModule : IModemModule, IDisposable {
		public bool Enabled => _options.Enabled;
		public bool IsInitialized { get; private set; }
		public bool Connected => _serverReaderWriter != null;

		private readonly ILogger<IModemModule> _logger;
		private readonly SerialPort _serialPort;
		private readonly ModemModuleOptions _options;

		private readonly IResponseValidator _responseValidator;

		private CryptoStreamReaderWriter _serverReaderWriter;

		public ModemModule(IOptions<ModemModuleOptions> options, ILogger<IModemModule> logger, IResponseValidator responseValidator) {
			_logger = logger;
			_options = options.Value;
			_responseValidator = responseValidator;

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

		public bool SendCommand(string command, bool throwOnFail = false, bool clearBuffer = true) {
			var responseLines = new List<string>();

			if (_serialPort.BytesToRead > 0 && clearBuffer) {
				_logger.LogWarning("Serial port has {BytesToRead} bytes to read - clearing buffer", _serialPort.BytesToRead);

				do {
					ReadLine();
				} while (_serialPort.BytesToRead > 0);
			}

			try {
				WriteLine(command);

				do {
					Thread.Sleep(5);
					responseLines.Add(ReadLine());
				} while (_serialPort.BytesToRead > 0);
			}
			catch (Exception ex) {
				string response = JoinResponse(responseLines);

				if (throwOnFail) {
					throw new SendCommandException($"Command {command} failed with response {response}", ex);
				}
				else {
					_logger.LogError(ex, "Command {Command} failed with response {Response}", command, response);
				}
			}

			bool containsExpectedResponse = _responseValidator.Validate(command, responseLines);

			if (throwOnFail && containsExpectedResponse == false) {
				throw new SendCommandException($"Command {command} failed with response {JoinResponse(responseLines)}");
			}

			return containsExpectedResponse;
		}

		public async Task<bool> WaitUntilExpectedResponse(string command, int timeoutSeconds, CancellationToken cancellationToken = default) {
			DateTime start = DateTime.Now;

			while (SendCommand(command) == false && cancellationToken.IsCancellationRequested == false) {
				if ((start - DateTime.Now).TotalSeconds < timeoutSeconds) {
					return false;
				}

				await Task.Delay(250, cancellationToken);
			}

			return true;
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

		private static string JoinResponse(List<string> responseLines) {
			string joinedResponse = string.Join(string.Empty, responseLines);
			if (joinedResponse == string.Empty) {
				joinedResponse = "<empty>";
			}

			return joinedResponse;
		}

		public Task InitializeAsync(CancellationToken cancellationToken = default) {
			return Task.Run(() => {
				InitializeBaudRate();
				SendCommand("AT+IFC=2,2", throwOnFail: true);
				SendCommand("AT+CMEE=2", throwOnFail: true);
				SendCommand("AT+CFUN=0", throwOnFail: true);
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
			await WaitUntilExpectedResponse("AT+CFUN?", 10);
			SendCommand("AT+COPS=0", throwOnFail: true);
			SendCommand("AT+CEREG=1", throwOnFail: true);
			SendCommand("AT+CGACT=1", throwOnFail: true);
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			_serialPort.Close();
			_serialPort.Dispose();
		}
	}
}

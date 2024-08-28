using Microsoft.Extensions.Options;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Protocols;
using RaspberryPi.ModemServer.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.ModemServer {
	public class ModemServerModule : IModemServerModule {
		public bool Enabled => _options.Enabled;
		public bool IsInitialized { get; private set; }

		private readonly ModemServerModuleOptions _options;
		private readonly IModemModule _modem;
		private readonly IServerProtocol _protocol;

		public ModemServerModule(IOptions<ModemServerModuleOptions> options, IModemModule modem, IServerProtocol protocol) {
			_options = options.Value;
			_modem = modem;
			_protocol = protocol;
		}

		public async Task InitializeAsync(CancellationToken cancellationToken = default) {
			await Task.Delay(1000, cancellationToken);
		}

		public async Task ConnectAsync() {
			_modem.SendCommand("AT+CIPMODE=1", throwOnFail: true);
			_modem.SendCommand("AT+CSTT=\"internet\"", throwOnFail: true);
			_modem.SendCommand("AT+CIICR", throwOnFail: true);
			_modem.SendCommand("AT+CIFSR", throwOnFail: true);
			_modem.SendCommand($"AT+CIPSERVER=1,{_options.ServerPort}", throwOnFail: true);

			_serverReaderWriter = await _protocol.InitializeCommunicationAsync(_serialPort.BaseStream, cancellationToken) as CryptoStreamReaderWriter
				?? throw new InvalidOperationException("Protocol returned stream of wrong type");
		}

		public async Task<byte[]> ReadAsync(CancellationToken cancellationToken = default) {
			AssertConnected();
			return await _serverReaderWriter.ReadMessageAsync(cancellationToken);
		}

		public async Task<string> ReadLineAsync(CancellationToken cancellationToken = default) {
			AssertConnected();
			return await _serverReaderWriter.ReadLineAsync(cancellationToken);
		}

		public async Task SendAsync(byte[] data, CancellationToken cancellationToken = default) {
			AssertConnected();
			await _serverReaderWriter.WriteMessageAsync(data, cancellationToken);
		}

		public async Task SendAsync(string data, CancellationToken cancellationToken = default) {
			AssertConnected();
			await _serverReaderWriter.WriteLineAsync(data, cancellationToken);
		}

		private void AssertConnected() {
			if (Connected == false) {
				throw new InvalidOperationException("Not connected to a server");
			}
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
	}
}

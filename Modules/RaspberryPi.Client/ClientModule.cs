using GorudoYami.Common.Cryptography;
using GorudoYami.Common.Streams;
using Microsoft.Extensions.Options;
using RaspberryPi.Client.Options;
using RaspberryPi.Common.Exceptions;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Common.Utilities;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Client {
	public class ClientModule : IClientModule, IDisposable, IAsyncDisposable {
		public bool Enabled => _options.Enabled;
		public bool IsInitialized { get; private set; }
		public IPAddress ServerAddress => Networking.GetAddressFromHostname(_options.ServerHost);
		public bool Connected => _mainServer?.Connected ?? false;

		private readonly ClientModuleOptions _options;
		private readonly IClientProtocol _protocol;
		private TcpClient _mainServer;
		private TcpClient _videoServer;
		private ByteStreamReaderWriter _mainServerReaderWriter;
		private ByteStreamReaderWriter _videoServerReaderWriter;

		public ClientModule(IOptions<ClientModuleOptions> options, IClientProtocol protocol) {
			_options = options.Value;
			_protocol = protocol;
			_mainServer = new TcpClient() {
				ReceiveTimeout = _options.TimeoutSeconds * 1000,
				SendTimeout = _options.TimeoutSeconds * 1000,
			};
			_videoServer = new TcpClient() {
				ReceiveTimeout = _options.TimeoutSeconds * 1000,
				SendTimeout = _options.TimeoutSeconds * 1000,
			};
		}

		public Task InitializeAsync(CancellationToken cancellationToken = default) {
			IsInitialized = true;
			return Task.CompletedTask;
		}

		public async Task ConnectAsync(CancellationToken cancellationToken = default) {
			if (_mainServer?.Connected == true) {
				await DisconnectAsync();
			}

			_mainServer = new TcpClient() {
				ReceiveTimeout = _options.TimeoutSeconds * 1000,
				SendTimeout = _options.TimeoutSeconds * 1000,
			};

			var timeoutTask = Task.Delay(_options.TimeoutSeconds * 1000, cancellationToken);
			Task connectTask = _mainServer.ConnectAsync(_options.ServerHost, _options.MainServerPort);
			await Task.WhenAny(timeoutTask, connectTask);

			if (_mainServer.Connected == false) {
				throw new InitializeCommunicationException($"Could not connect to the server at {_options.ServerHost}:{_options.MainServerPort}");
			}

			_mainServerReaderWriter = new ByteStreamReaderWriter(_mainServer.GetStream());
		}

		public async Task ConnectVideoAsync(CancellationToken cancellationToken = default) {
			if (_mainServer?.Connected != true) {
				throw new InvalidOperationException("Not connected to the main server");
			}

			if (_videoServer?.Connected == true) {
				await DisconnectVideoAsync();
			}

			_videoServer = new TcpClient() {
				ReceiveTimeout = _options.TimeoutSeconds * 1000,
				SendTimeout = _options.TimeoutSeconds * 1000,
			};

			var timeoutTask = Task.Delay(_options.TimeoutSeconds * 1000, cancellationToken);
			Task connectTask = _videoServer.ConnectAsync(_options.ServerHost, _options.VideoServerPort);
			await Task.WhenAny(timeoutTask, connectTask);

			if (_videoServer.Connected == false) {
				throw new InitializeCommunicationException($"Could not connect to the video server at {_options.ServerHost}:{_options.VideoServerPort}");
			}

			_videoServerReaderWriter = new ByteStreamReaderWriter(_videoServer.GetStream());
		}

		public async Task SendAsync(
			byte[] data,
			CancellationToken cancellationToken = default) {
			AssertConnected();
			await _mainServerReaderWriter.WriteMessageAsync(data, cancellationToken);
		}

		public async Task SendAsync(
			string data,
			CancellationToken cancellationToken = default) {
			await SendAsync(Encoding.UTF8.GetBytes(data), cancellationToken);
		}

		public async Task<string> ReadLineAsync(CancellationToken cancellationToken = default) {
			return Encoding.UTF8.GetString(await ReadAsync(cancellationToken));
		}

		public async Task<byte[]> ReadAsync(CancellationToken cancellationToken = default) {
			AssertConnected();
			return await _mainServerReaderWriter.ReadMessageAsync(cancellationToken);
		}

		private void AssertConnected() {
			if (_mainServer?.Connected != true) {
				throw new InvalidOperationException("Not connected to a server");
			}
		}

		public async Task DisconnectAsync() {
			if (_mainServerReaderWriter != null) {
				await _mainServerReaderWriter.DisposeAsync();
				_mainServerReaderWriter = null;
			}

			_mainServer?.Dispose();
			_mainServer = null;
		}

		public async Task DisconnectVideoAsync() {
			if (_videoServerReaderWriter != null) {
				await _videoServerReaderWriter.DisposeAsync();
				_videoServerReaderWriter = null;
			}

			_videoServer?.Dispose();
			_videoServer = null;
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			DisconnectVideoAsync().GetAwaiter().GetResult();
			DisconnectAsync().GetAwaiter().GetResult();
		}

		public async ValueTask DisposeAsync() {
			GC.SuppressFinalize(this);
			await DisconnectVideoAsync();
			await DisconnectAsync();
		}
	}
}

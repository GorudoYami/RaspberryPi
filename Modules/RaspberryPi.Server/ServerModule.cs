using GorudoYami.Common.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Common.Utilities;
using RaspberryPi.Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Server {
	public class ServerModule : IServerModule, IDisposable, IAsyncDisposable {
		public bool Enabled { get; }
		public bool IsInitialized { get; private set; }

		private readonly Dictionary<IPAddress, TcpClientInfo> _clients;
		private readonly TcpListener _mainListener;
		private readonly TcpListener _videoListener;
		private readonly ILogger<IServerModule> _logger;
		private readonly IServerProtocol _protocol;
		private CancellationTokenSource _cancellationTokenSource;
		private Task _mainListenTask;
		private Task _videoListenTask;
		private Task _readMessagesTask;

		public ServerModule(IOptions<ServerModuleOptions> options, ILogger<IServerModule> logger, IServerProtocol protocol) {
			_logger = logger;
			_clients = new Dictionary<IPAddress, TcpClientInfo>();
			_mainListener = new TcpListener(Networking.GetAddressFromHostname(options.Value.Host), options.Value.MainPort);
			_videoListener = new TcpListener(Networking.GetAddressFromHostname(options.Value.Host), options.Value.VideoPort);
			_protocol = protocol;
		}

		public Task InitializeAsync(CancellationToken cancellationToken = default) {
			IsInitialized = true;
			return Task.CompletedTask;
		}

		public void Start() {
			if (_cancellationTokenSource == null) {
				_cancellationTokenSource = new CancellationTokenSource();
			}

			_mainListener.Start();
			_mainListenTask = MainListenAsync(_cancellationTokenSource.Token);
			_videoListener.Start();
			_videoListenTask = VideoListenAsync(_cancellationTokenSource.Token);
			_readMessagesTask = ReadMessagesAsync(_cancellationTokenSource.Token);
		}

		public async Task StopAsync() {
			if (_mainListenTask?.Status != TaskStatus.Running &&
				_videoListenTask?.Status != TaskStatus.Running) {
				return;
			}

			try {
				_cancellationTokenSource?.Cancel();
				_mainListener.Stop();
				_videoListener.Stop();
				await _mainListenTask;
				await _videoListenTask;
				await _readMessagesTask;
			}
			finally {
				Cleanup();
			}
		}

		private void Cleanup() {
			foreach (IPAddress address in _clients.Keys) {
				CleanupClient(address);
			}

			_clients.Clear();
			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;
		}

		private void CleanupClient(IPAddress address, bool remove = false) {
			if (_clients.ContainsKey(address)) {
				_clients[address].Dispose();

				if (remove) {
					_clients.Remove(address);
				}
			}
		}

		public async Task BroadcastAsync(
			string data,
			CancellationToken cancellationToken = default) {
			await BroadcastAsync(Encoding.UTF8.GetBytes(data), cancellationToken);
		}

		public async Task BroadcastAsync(
			byte[] data,
			CancellationToken cancellationToken = default) {
			await Task.WhenAll(
				_clients.Keys.Select(x => SendAsync(x, data, cancellationToken))
			);
		}

		public async Task SendAsync(
			IPAddress address,
			byte[] data,
			CancellationToken cancellationToken = default) {
			if (_clients.ContainsKey(address) == false) {
				throw new InvalidOperationException($"Client {address} is not connected");
			}

			await _clients[address].MainReaderWriter.WriteMessageAsync(data, cancellationToken);
		}

		public async Task BroadcastVideoAsync(
			byte[] data,
			CancellationToken cancellationToken = default) {
			await Task.WhenAll(
				_clients.Keys.Select(x => SendVideoAsync(x, data, cancellationToken))
			);
		}

		private async Task SendVideoAsync(
			IPAddress address,
			byte[] data,
			CancellationToken cancellationToken = default) {
			if (_clients.ContainsKey(address) == false) {
				throw new InvalidOperationException($"Client {address} is not connected");
			}

			TcpClientInfo clientInfo = _clients[address];
			if (clientInfo.VideoReaderWriter == null) {
				throw new InvalidOperationException($"Client {address} is not connected to video");
			}

			await clientInfo.VideoReaderWriter.WriteMessageAsync(data, cancellationToken);
		}

		private async Task MainListenAsync(CancellationToken cancellationToken) {
			while (cancellationToken.IsCancellationRequested == false) {
				TcpClient client = await _mainListener.AcceptTcpClientAsync();
				IPAddress clientAddress = (client.Client.RemoteEndPoint as IPEndPoint)?.Address
					?? throw new InvalidOperationException("Client remote endpoint is invalid");

				if (client.Connected) {
					try {
						_clients[clientAddress] = new TcpClientInfo(client, _protocol.Delimiter);
					}
					catch (Exception ex) {
						_logger.LogError(ex, "Communication initialization with client {ClientAddress} failed", clientAddress.ToString());
						CleanupClient(clientAddress, true);
					}
				}
			}
		}

		private async Task VideoListenAsync(CancellationToken cancellationToken) {
			while (cancellationToken.IsCancellationRequested == false) {
				TcpClient client = await _videoListener.AcceptTcpClientAsync();
				IPAddress clientAddress = (client.Client.RemoteEndPoint as IPEndPoint)?.Address
					?? throw new InvalidOperationException("Client remote endpoint is invalid");

				if (client.Connected) {
					try {
						if (_clients.ContainsKey(clientAddress) == false) {
							_logger.LogError("Client {ClientAddress} is not connected to main server", clientAddress.ToString());
						}
						else {
							TcpClientInfo clientInfo = _clients[clientAddress];
							clientInfo.SetVideoClient(client);
						}
					}
					catch (Exception ex) {
						_logger.LogError(ex, "Communication initialization with client {ClientAddress} failed", clientAddress.ToString());
						client.Dispose();
					}
				}
			}
		}

		private async Task ReadMessagesAsync(CancellationToken cancellationToken) {
			while (cancellationToken.IsCancellationRequested == false) {
				try {
					await Task.WhenAll(_clients.Values.Select(x => ReadIncomingMessage(x, cancellationToken)));
				}
				catch (Exception ex) {
					_logger.LogError(ex, "Error occured while reading messages from clients");
				}
			}
		}

		private async Task ReadIncomingMessage(TcpClientInfo clientInfo, CancellationToken cancellationToken) {
			byte[] message = await clientInfo.MainReaderWriter.ReadMessageAsync(cancellationToken);

			if (message?.Length > 0) {
				_protocol.ParseMessage(message);
			}
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			StopAsync().GetAwaiter().GetResult();
		}

		public async ValueTask DisposeAsync() {
			GC.SuppressFinalize(this);
			await StopAsync();
		}
	}
}

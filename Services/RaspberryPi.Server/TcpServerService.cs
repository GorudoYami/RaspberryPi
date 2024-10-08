﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Common.Services;
using RaspberryPi.TcpServer.Models;
using RaspberryPi.TcpServer.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.TcpServer {
	public class TcpServerService
		: ITcpServerService, IDisposable {
		public bool Enabled { get; }

		private readonly Dictionary<IPAddress, TcpClientInfo> _clients = new Dictionary<IPAddress, TcpClientInfo>();
		private readonly TcpListener _listener;
		private readonly ILogger<ITcpServerService> _logger;
		private readonly ICommunicationProtocol _communicationProtocol;
		private CancellationTokenSource _cancellationTokenSource;
		private Task _listenTask;
		private Task _readMessagesTask;

		public TcpServerService(IOptions<TcpServerOptions> options, ILogger<ITcpServerService> logger, ICommunicationProtocol communicationProtocol) {
			_listener = new TcpListener(IPAddress.Any, options.Value.MainPort);
			_logger = logger;
			_communicationProtocol = communicationProtocol;
		}

		public void Start() {
			if (_cancellationTokenSource == null) {
				_cancellationTokenSource = new CancellationTokenSource();
			}

			_listener.Start();
			_listenTask = ListenAsync(_cancellationTokenSource.Token);
			_readMessagesTask = Task.Run(() => ReadMessagesAsync(_cancellationTokenSource.Token));
		}

		public async Task StopAsync() {
			if (_listenTask?.Status != TaskStatus.Running) {
				return;
			}

			try {
				_cancellationTokenSource?.Cancel();
				_listener.Stop();
				await _listenTask;
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
			if (_clients.TryGetValue(address, out TcpClientInfo value)) {
				value?.Dispose();

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

			await _clients[address].IO.WriteMessageAsync(data, cancellationToken);
		}

		private async Task ListenAsync(CancellationToken cancellationToken) {
			while (cancellationToken.IsCancellationRequested == false) {
				TcpClient client = await _listener.AcceptTcpClientAsync();
				IPAddress clientAddress = (client.Client.RemoteEndPoint as IPEndPoint)?.Address
					?? throw new InvalidOperationException("Client remote endpoint is invalid");

				if (client.Connected) {
					_logger.LogInformation("Client connected {ClientAddress}", clientAddress);
					try {
						_clients[clientAddress] = new TcpClientInfo(client, _communicationProtocol.Delimiter);
					}
					catch (Exception ex) {
						_logger.LogError(ex, "Communication initialization with client {ClientAddress} failed", clientAddress.ToString());
						CleanupClient(clientAddress, true);
					}
				}
			}
		}

		private async Task ReadMessagesAsync(CancellationToken cancellationToken) {
			while (cancellationToken.IsCancellationRequested == false) {
				try {
					await Task.WhenAll(_clients.Select(client => ReadIncomingMessage(client.Key, client.Value, cancellationToken)));
					await Task.Delay(1, cancellationToken);
				}
				catch (Exception ex) {
					_logger.LogError(ex, "Error occured while reading messages from clients: {ExceptionMessage}", ex.Message);
				}
			}
		}

		private async Task ReadIncomingMessage(IPAddress clientAddress, TcpClientInfo clientInfo, CancellationToken cancellationToken) {
			if (clientInfo.Client.Connected == false) {
				CleanupClient(clientAddress, true);
				return;
			}

			byte[] message = await clientInfo.IO.ReadMessageAsync(cancellationToken);

			if (message?.Length > 0) {
				string messageStr = $"[{string.Join(", ", message.Select(x => x.ToString(Thread.CurrentThread.CurrentCulture)).ToArray())}]";
				_logger.LogTrace("Received {Message} from client {ClientAddress}", messageStr, clientAddress);
				_communicationProtocol.ParseMessage(message);
			}
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			StopAsync().GetAwaiter().GetResult();
		}
	}
}

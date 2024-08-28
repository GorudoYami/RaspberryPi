using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Common.Services;
using RaspberryPi.Common.Utilities;
using RaspberryPi.TcpServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.TcpServer;
public class TcpServerService : ITcpServerService, IDisposable, IAsyncDisposable {
	public bool Enabled { get; }

	private readonly Dictionary<IPAddress, TcpClientInfo> _clients;
	private readonly TcpListener _listener;
	private readonly ILogger<ITcpServerService> _logger;
	private readonly IServerProtocol _protocol;
	private CancellationTokenSource _cancellationTokenSource;
	private Task _listenTask;
	private Task _readMessagesTask;

	public TcpServerService(IOptions<TcpServerModuleOptions> options, ILogger<ITcpServerService> logger, IServerProtocol protocol) {
		_logger = logger;
		_clients = [];
		_listener = new TcpListener(Networking.GetAddressFromHostname(options.Value.Host), options.Value.MainPort);
		_protocol = protocol;
	}

	public void Start() {
		_cancellationTokenSource ??= new CancellationTokenSource();

		_listener.Start();
		_listenTask = ListenAsync(_cancellationTokenSource.Token);
		_readMessagesTask = ReadMessagesAsync(_cancellationTokenSource.Token);
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

		await _clients[address].IO.WriteMessageAsync(data, cancellationToken);
	}

	private async Task ListenAsync(CancellationToken cancellationToken) {
		while (cancellationToken.IsCancellationRequested == false) {
			TcpClient client = await _listener.AcceptTcpClientAsync();
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
		byte[] message = await clientInfo.IO.ReadMessageAsync(cancellationToken);

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

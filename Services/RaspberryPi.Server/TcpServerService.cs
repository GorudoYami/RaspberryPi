﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Common.Services;
using RaspberryPi.Common.Utilities;
using RaspberryPi.TcpServer.Models;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RaspberryPi.TcpServer;

public class TcpServerService(
	IOptions<TcpServerModuleOptions> options,
	ILogger<ITcpServerService> logger,
	IServerProtocol protocol)
	: ITcpServerService, IDisposable, IAsyncDisposable {
	public bool Enabled { get; }

	private readonly Dictionary<IPAddress, TcpClientInfo> _clients = [];
	private readonly TcpListener _listener = new(Networking.GetAddressFromHostname(options.Value.Host), options.Value.MainPort);
	private CancellationTokenSource? _cancellationTokenSource;
	private Task? _listenTask;
	private Task? _readMessagesTask;

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
			await _readMessagesTask!;
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
		if (_clients.TryGetValue(address, out TcpClientInfo? value)) {
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
			TcpClient client = await _listener.AcceptTcpClientAsync(cancellationToken);
			IPAddress clientAddress = (client.Client.RemoteEndPoint as IPEndPoint)?.Address
				?? throw new InvalidOperationException("Client remote endpoint is invalid");

			if (client.Connected) {
				try {
					_clients[clientAddress] = new TcpClientInfo(client, protocol.Delimiter);
				}
				catch (Exception ex) {
					logger.LogError(ex, "Communication initialization with client {ClientAddress} failed", clientAddress.ToString());
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
				logger.LogError(ex, "Error occured while reading messages from clients");
			}
		}
	}

	private async Task ReadIncomingMessage(TcpClientInfo clientInfo, CancellationToken cancellationToken) {
		byte[] message = await clientInfo.IO.ReadMessageAsync(cancellationToken);

		if (message?.Length > 0) {
			protocol.ParseMessage(message);
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

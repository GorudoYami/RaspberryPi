using GorudoYami.Common.Cryptography;
using GorudoYami.Common.Streams;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Common.Utilities;
using RaspberryPi.Server.Models;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RaspberryPi.Server;

public class ServerModule : IServerModule, IDisposable, IAsyncDisposable {
	public bool LazyInitialization => true;
	public bool IsInitialized { get; private set; }

	private readonly Dictionary<IPAddress, TcpClientInfo> _clients;
	private readonly TcpListener _listener;
	private readonly ILogger<IServerModule> _logger;
	private readonly IProtocol _protocol;
	private CancellationTokenSource? _cancellationTokenSource;
	private Task? _listenTask;

	public ServerModule(IOptions<ServerModuleOptions> options, ILogger<IServerModule> logger, IServerProtocol protocol) {
		_logger = logger;
		_clients = [];
		_listener = new TcpListener(Networking.GetAddressFromHostname(options.Value.Host), options.Value.Port);
		_protocol = protocol;
	}

	public Task InitializeAsync(CancellationToken cancellationToken = default) {
		return Task.Run(Start, cancellationToken);
	}

	public void Start() {
		_cancellationTokenSource ??= new CancellationTokenSource();
		_listener.Start();
		_listenTask = ListenAsync(_cancellationTokenSource.Token);
	}

	public async Task StopAsync() {
		if (_listenTask?.Status != TaskStatus.Running) {
			return;
		}

		try {
			_cancellationTokenSource?.Cancel();
			_listener.Stop();
			await _listenTask;
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
		bool encrypt = true,
		CancellationToken cancellationToken = default) {
		await BroadcastAsync(Encoding.UTF8.GetBytes(data), encrypt, cancellationToken);
	}

	public async Task BroadcastAsync(
		byte[] data,
		bool encrypt = true,
		CancellationToken cancellationToken = default) {
		await Task.WhenAll(
			_clients.Keys.Select(x => SendAsync(x, data, encrypt, cancellationToken))
		);
	}

	public async Task SendAsync(
		IPAddress address,
		byte[] data,
		bool encrypt = true,
		CancellationToken cancellationToken = default) {
		if (_clients.ContainsKey(address) == false) {
			throw new InvalidOperationException($"Client {address} is not connected");
		}

		if (encrypt) {
			await _clients[address].ReaderWriter.WriteMessageAsync(data, cancellationToken);
		}
		else {
			await _clients[address].Stream.WriteAsync(data, cancellationToken);
			await _clients[address].Stream.WriteAsync(Encoding.UTF8.GetBytes("\r\n"), cancellationToken);
		}
	}

	private async Task ListenAsync(CancellationToken cancellationToken) {
		while (cancellationToken.IsCancellationRequested == false) {
			TcpClient client = await _listener.AcceptTcpClientAsync(cancellationToken);
			IPAddress clientAddress = (client.Client.RemoteEndPoint as IPEndPoint)?.Address
				?? throw new InvalidOperationException("Client remote endpoint is invalid");

			if (client.Connected) {
				try {
					var clientStream = await _protocol.InitializeCommunicationAsync(client.GetStream(), cancellationToken) as CryptoStreamReaderWriter
						?? throw new InvalidOperationException("Protocol returned stream of wrong type");

					_clients[clientAddress] = new TcpClientInfo(client, clientStream);
				}
				catch (Exception ex) {
					_logger.LogError(ex, "Communication initialization with client {ClientAddress} failed", clientAddress.ToString());
					CleanupClient(clientAddress, true);
				}
			}
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

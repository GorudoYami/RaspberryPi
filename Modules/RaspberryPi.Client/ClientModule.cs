using GorudoYami.Common.Cryptography;
using Microsoft.Extensions.Options;
using RaspberryPi.Client.Models;
using RaspberryPi.Common.Exceptions;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Common.Utilities;
using System.Net;
using System.Net.Sockets;

namespace RaspberryPi.Client;

public class ClientModule : IClientModule, IDisposable, IAsyncDisposable {
	public bool LazyInitialization => false;
	public bool IsInitialized { get; private set; }
	public IPAddress ServerAddress => Networking.GetAddressFromHostname(_options.ServerHost);
	public bool Connected => _server?.Connected ?? false;

	private readonly ClientModuleOptions _options;
	private readonly IProtocol _protocol;
	private TcpClient? _server;
	private CryptoStreamReaderWriter? _serverReaderWriter;

	public ClientModule(IOptions<ClientModuleOptions> options, IClientProtocol protocol) {
		_options = options.Value;
		_protocol = protocol;
		_server = new TcpClient() {
			ReceiveTimeout = _options.TimeoutSeconds * 1000,
			SendTimeout = _options.TimeoutSeconds * 1000,
		};
	}

	public async Task InitializeAsync(CancellationToken cancellationToken = default) {
		await Task.Delay(0, cancellationToken);
		IsInitialized = true;
	}

	public async Task ConnectAsync(CancellationToken cancellationToken = default) {
		if (_server?.Connected == true) {
			await DisconnectAsync(cancellationToken);
		}

		_server = new TcpClient() {
			ReceiveTimeout = _options.TimeoutSeconds * 1000,
			SendTimeout = _options.TimeoutSeconds * 1000,
		};

		var timeoutTask = Task.Delay(_options.TimeoutSeconds * 1000, cancellationToken);
		Task connectTask = _server.ConnectAsync(_options.ServerHost, _options.ServerPort);
		await Task.WhenAny(timeoutTask, connectTask);

		if (_server.Connected == false) {
			throw new InitializeCommunicationException($"Could not connect to the server at {_options.ServerHost}");
		}
		else {
			_serverReaderWriter = await _protocol.InitializeCommunicationAsync(_server.GetStream(), cancellationToken) as CryptoStreamReaderWriter
				?? throw new InvalidOperationException("Protocol returned stream of wrong type");
		}
	}

	public async Task SendAsync(
		byte[] data,
		CancellationToken cancellationToken = default) {
		AssertConnected();
		await _serverReaderWriter!.WriteMessageAsync(data, cancellationToken);
	}

	public async Task SendAsync(
		string data,
		CancellationToken cancellationToken = default) {
		AssertConnected();
		await _serverReaderWriter!.WriteLineAsync(data, cancellationToken);
	}

	public async Task<string> ReadLineAsync(CancellationToken cancellationToken = default) {
		AssertConnected();
		return await _serverReaderWriter!.ReadLineAsync(cancellationToken);
	}

	public async Task<byte[]> ReadAsync(CancellationToken cancellationToken = default) {
		AssertConnected();
		return await _serverReaderWriter!.ReadMessageAsync(cancellationToken);
	}

	private void AssertConnected() {
		if (_server?.Connected != true) {
			throw new InvalidOperationException("Not connected to a server");
		}
	}

	public async Task DisconnectAsync(CancellationToken cancellationToken = default) {
		if (_serverReaderWriter != null) {
			await _serverReaderWriter.DisposeAsync();
			_serverReaderWriter = null;
		}

		_server?.Dispose();
		_server = null;
	}

	public void Dispose() {
		GC.SuppressFinalize(this);
		DisconnectAsync().GetAwaiter().GetResult();
	}

	public async ValueTask DisposeAsync() {
		GC.SuppressFinalize(this);
		await DisconnectAsync();
	}
}

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using RaspberryPi.Common;
using GorudoYami.Common.Networking;
using Microsoft.Extensions.Options;
using GorudoYami.Common.Asynchronous;
using GorudoYami.Common.Modules;
using RaspberryPi.Modules.Models;

namespace RaspberryPi.Modules;

public interface ITcpServerModule : IModule {
	Task BroadcastAsync(string data, bool encrypt = true, CancellationToken cancellationToken = default);
	Task BroadcastAsync(byte[] data, bool encrypt = true, CancellationToken cancellationToken = default);
	Task SendAsync(IPAddress address, byte[] data, bool encrypt = true, CancellationToken cancellationToken = default);
	void Start();
	Task StopAsync();
}

public class TcpServerModule : ITcpServerModule, IDisposable, IAsyncDisposable {
	private const int _rsaKeySizeBits = 8000;
	private const int _aesKeySizeBits = 256;
	private const int _aesIVSizeBits = 128;

	private readonly Dictionary<IPAddress, TcpClientInfo> _clients;
	private readonly TcpListener _listener;
	private readonly ICancellationTokenProvider _cancellationTokenProvider;
	private Task? _listenTask;

	public TcpServerModule(IOptions<TcpServerModuleOptions> options, ICancellationTokenProvider cancellationTokenProvider) {
		_cancellationTokenProvider = cancellationTokenProvider;
		_clients = new Dictionary<IPAddress, TcpClientInfo>();
		_listener = new TcpListener(Networking.GetAddressFromHostname(options.Value.Host), options.Value.Port);
	}

	public void Start() {
		_listener.Start();
		_listenTask = ListenAsync(_cancellationTokenProvider.GetToken());
	}

	public async Task StopAsync() {
		if (_listenTask?.Status != TaskStatus.Running) {
			throw new InvalidOperationException("Listen task is not running");
		}

		_cancellationTokenProvider.Cancel();
		await _listenTask;
		Cleanup();
	}

	private void Cleanup() {
		foreach (IPAddress address in _clients.Keys) {
			CleanupClient(address);
		}

		_clients.Clear();
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
			await _clients[address].IO.WriteMessageAsync(data, cancellationToken);
		}
		else {
			await _clients[address].NetworkStream.WriteAsync(data, cancellationToken);
			await _clients[address].NetworkStream.WriteAsync(Encoding.UTF8.GetBytes("\r\n"), cancellationToken);
		}
	}

	private async Task ListenAsync(CancellationToken token) {
		while (!token.IsCancellationRequested) {
			TcpClient client = await _listener.AcceptTcpClientAsync(token);
			IPAddress clientAddress = (client.Client.RemoteEndPoint as IPEndPoint)?.Address
				?? throw new InvalidOperationException("Client remote endpoint is invalid");

			if (client.Connected && await InitializeCommunicationAsync(client, clientAddress) == false) {
				CleanupClient(clientAddress, true);
			}
		}
	}

	private async Task<bool> InitializeCommunicationAsync(TcpClient client, IPAddress clientAddress) {
		using var clientStream = client.GetStream();
		using var clientReader = new ByteStreamReader(clientStream, true);

		using RSA rsa = RSA.Create(_rsaKeySizeBits);

		byte[] buffer = await clientReader.ReadMessageAsync(cancellationToken: _cancellationTokenProvider.GetToken());
		if (buffer == null || buffer.Length < _rsaKeySizeBits / 8) {
			return false;
		}

		rsa.ImportRSAPublicKey(buffer, out int bytesRead);
		if (bytesRead != buffer.Length) {
			return false;
		}

		Aes aes = GetAes();
		byte[] data = rsa.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA512);
		await clientStream.WriteAsync(data, _cancellationTokenProvider.GetToken());
		await clientStream.WriteAsync(Encoding.ASCII.GetBytes("\r\n"), _cancellationTokenProvider.GetToken());

		data = rsa.Encrypt(aes.IV, RSAEncryptionPadding.OaepSHA512);
		await clientStream.WriteAsync(data, _cancellationTokenProvider.GetToken());
		await clientStream.WriteAsync(Encoding.ASCII.GetBytes("\r\n"), _cancellationTokenProvider.GetToken());

		var csrwClient = new CryptoStreamReaderWriter(aes.CreateEncryptor(), aes.CreateDecryptor(), client.GetStream());
		if (await csrwClient.ReadLineAsync() == "OK") {
			_clients.Add(clientAddress, new TcpClientInfo(client, csrwClient));
			return true;
		}
		else {
			return false;
		}
	}

	private static Aes GetAes() {
		Aes aes = Aes.Create();
		aes.KeySize = _aesKeySizeBits;
		aes.Key = RandomNumberGenerator.GetBytes(_aesKeySizeBits / 8);
		aes.IV = RandomNumberGenerator.GetBytes(_aesIVSizeBits / 8);
		return aes;
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

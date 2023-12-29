using GorudoYami.Common.Cryptography;
using GorudoYami.Common.Streams;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Exceptions;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Utilities;
using RaspberryPi.Server.Models;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace RaspberryPi.Server;

public class ServerModule : IServerModule, IDisposable, IAsyncDisposable {
	public bool LazyInitialization => true;
	public bool IsInitialized { get; private set; }

	private readonly Dictionary<IPAddress, TcpClientInfo> _clients;
	private readonly TcpListener _listener;
	private readonly ILogger<IServerModule> _logger;
	private CancellationTokenSource? _cancellationTokenSource;
	private Task? _listenTask;

	public ServerModule(IOptions<ServerModuleOptions> options, ILogger<IServerModule> logger) {
		_logger = logger;
		_clients = [];
		_listener = new TcpListener(Networking.GetAddressFromHostname(options.Value.Host), options.Value.Port);
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
					await InitializeCommunicationAsync(client, clientAddress, cancellationToken);
				}
				catch (Exception ex) {
					_logger.LogError(ex, "Communication initialization with client {ClientAddress} failed", clientAddress.ToString());
					CleanupClient(clientAddress, true);
				}
			}
		}
	}

	private async Task InitializeCommunicationAsync(TcpClient client, IPAddress clientAddress, CancellationToken cancellationToken) {
		NetworkStream clientStream = client.GetStream();
		using var clientReader = new ByteStreamReader(clientStream, true);
		using var rsa = RSA.Create(CryptographyKeySizes.RsaKeySizeBits);

		byte[] data = await clientReader.ReadMessageAsync(cancellationToken: cancellationToken);
		int expectedLength = CryptographyKeySizes.RsaKeySizeBits / 8 + CryptographyKeySizes.RsaKeyInfoSizeBits / 8;
		if (data.Length != expectedLength) {
			throw new InitializeCommunicationException($"Received public key has an invalid size. Expected: {expectedLength}. Actual: {data.Length}.");
		}

		rsa.ImportRSAPublicKey(data, out int bytesRead);
		if (bytesRead != data.Length) {
			throw new InitializeCommunicationException($"Public key has not been read fully. Expected: {data.Length}. Read: {bytesRead}.");
		}

		Aes? aes = null;
		CryptoStreamReaderWriter? csrwClient = null;
		bool result = false;
		try {
			aes = GetAes();

			data = rsa.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA512);
			await clientStream.WriteAsync(data, cancellationToken);
			await clientStream.WriteAsync(Encoding.ASCII.GetBytes("\r\n"), cancellationToken);

			data = rsa.Encrypt(aes.IV, RSAEncryptionPadding.OaepSHA512);
			await clientStream.WriteAsync(data, cancellationToken);
			await clientStream.WriteAsync(Encoding.ASCII.GetBytes("\r\n"), cancellationToken);

			csrwClient = new CryptoStreamReaderWriter(aes.CreateEncryptor(), aes.CreateDecryptor(), clientStream);
			string response = await csrwClient.ReadLineAsync(cancellationToken);
			if (response == "OK") {
				_clients.Add(clientAddress, new TcpClientInfo(client, csrwClient));
				result = true;
			}
			else {
				throw new InitializeCommunicationException($"Did not receive correct response. Expected: OK. Received: {response}");
			}
		}
		finally {
			if (result == false) {
				if (csrwClient != null) {
					await csrwClient.DisposeAsync();
				}

				aes?.Dispose();
			}
		}
	}

	private static Aes GetAes() {
		var aes = Aes.Create();
		aes.KeySize = CryptographyKeySizes.AesKeySizeBits;
		aes.Key = RandomNumberGenerator.GetBytes(CryptographyKeySizes.AesKeySizeBits / 8);
		aes.IV = RandomNumberGenerator.GetBytes(CryptographyKeySizes.AesIvSizeBits / 8);
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

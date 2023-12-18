using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using RaspberryPi.Common;
using RaspberryPi.Common.Interfaces;
using GorudoYami.Common.Cryptography;
using RaspberryPi.TcpServerModule.Models;

namespace RaspberryPi.TcpServerModule;

public interface ITcpServerModule : IRaspberryPiModule, IDisposable {

}

public class TcpServerModule : ITcpServerModule {
	private const int _rsaKeySize = 8000;
	private readonly IPAddress _address;
	private readonly int _port;
	private readonly Dictionary<TcpClient, ClientInfo> _clients;
	private readonly TcpListener _listener;
	private CancellationTokenSource _cancellationTokenSource;
	private Task? _listenTask;

	public TcpServerModule(string hostname, int port) {
		_clients = new Dictionary<TcpClient, ClientInfo>();
		_port = port;
		_address = Networking.GetAddressFromHostname(hostname);
		_listener = new TcpListener(_address, _port);
		_cancellationTokenSource = new CancellationTokenSource();
	}

	public void Start() {
		_listener.Start();
		_listenTask = ListenAsync(_cancellationTokenSource.Token);
	}

	public async Task StopAsync() {
		if (_listenTask?.Status != TaskStatus.Running) {
			return;
		}

		_cancellationTokenSource.Cancel();
		await _listenTask;
		Cleanup();
	}

	private void Cleanup() {
		_cancellationTokenSource.Dispose();

		foreach (TcpClient client in _clients.Keys) {
			CleanupClient(client);
		}

		_clients.Clear();
	}

	private static void CleanupClient(TcpClient client) {
		client.Close();
		client.Dispose();
	}

	public async Task<bool> BroadcastAsync(
		byte[] data,
		bool encrypt = true,
		CancellationToken cancellationToken = default) {
		IEnumerable<bool> results = await Task.WhenAll(
			_clients.Keys.Select(x => SendAsync(x, data, encrypt, cancellationToken))
		);

		return results.All(x => x);
	}

	public static async Task<bool> SendAsync(
		TcpClient client,
		byte[] data,
		bool encrypt = true,
		CancellationToken cancellationToken = default) {

		try {
			ICryptographyService cryptographyService = new CryptographyService();
			using var stream = client.GetStream();
			if (encrypt) {
				data = await cryptographyService.EncryptAsync(data, cancellationToken: cancellationToken);
			}
			await stream.WriteAsync(Encoding.UTF8.GetBytes("\r\n"), cancellationToken);
		}
		catch (Exception ex) {
			Console.WriteLine(ex.ToString());
			return false;
		}

		return true;
	}

	public async Task<byte[]> ReceiveAsync(byte[] data) {
		byte[] buffer = new byte[1024];

		bool timedOut = false;
		using var timer = new Timer(_client.ReceiveTimeout * 1000);
		timer.Elapsed += (s, e) => timedOut = true;
		timer.Start();

		// Receive data (wait until "\r\n" or timeout)
		try {
			using var stream = client.GetStream();

			while (!data.Contains("\r\n") && !timedOut) {
				if (client.Available > 0) {
					await stream.ReadAsync(buffer, token);
					data += Encoding.ASCII.GetString(buffer);
					Array.Clear(buffer, 0, buffer.Length);
				}
			}
		}
		catch (Exception ex) {
			Console.WriteLine(ex.ToString());
			return null;
		}

		if (timedOut)
			return null;

		// Remove last 2 characters ("\r\n")
		data = data[0..^2];

		return CryptoUtils.DecryptData(Encoding.ASCII.GetBytes(data), aes);
	}

	private async Task ListenAsync(CancellationToken token) {
		while (!token.IsCancellationRequested) {
			TcpClient client = await _listener.AcceptTcpClientAsync(token);

			if (client.Connected) {
				await InitializeCommunicationAsync(client);

				if (aes is not null)
					_clients.Add(client, aes);
				else
					CleanupClient(client);
			}
		}
	}

	private async Task InitializeCommunicationAsync(TcpClient client) {
		client.ReceiveTimeout = 15000;
		client.SendTimeout = 15000;


		using RSA rsa = RSA.Create(_rsaKeySize);

		// Receive client public key
		byte[] buffer = await client.ReceiveAsync(tokenSource.Token);
		if (buffer is null || buffer.Length < KeySizes.RSA_KEY_SIZE / 8)
			return null;

		// Import client public key
		rsa.ImportRSAPublicKey(buffer, out int bytesRead);
		if (bytesRead != buffer.Length)
			return null;

		Aes clientAes = CryptoUtils.CreateAes();

		// Send encrypted AES key and IV with RSA
		if (!await client.SendUnencryptedAsync(rsa.Encrypt(clientAes.Key, RSAEncryptionPadding.OaepSHA512), tokenSource.Token))
			return null;

		if (!await TcpUtils.SendUnencryptedAsync(client, rsa.Encrypt(clientAes.IV, RSAEncryptionPadding.OaepSHA512), tokenSource.Token))
			return null;

		// Receive secret application token
		buffer = await TcpUtils.ReceiveAsync(client, _clients[client], tokenSource.Token);

		return Encoding.ASCII.GetString(buffer) != _applicationToken ? null : clientAes;
	}

	public void Dispose() {
		StopAsync();
		GC.SuppressFinalize(this);
	}
}

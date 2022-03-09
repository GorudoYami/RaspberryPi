using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RaspberryPi.Common;
using KeySizes = RaspberryPi.Common.KeySizes;
using Timer = System.Timers.Timer;

namespace RaspberryPi;

public class RaspberryClient : IDisposable {
	public string ServerHostname { get; set; }
	public int ServerPort { get; set; }
	public ConcurrentQueue<byte[]> DataQueue { get; private set; }

	private TcpClient Client { get; set; }
	private CancellationTokenSource TokenSource { get; set; }
	private Aes ServerAes { get; set; }
	private string ApplicationToken { get; set; }
	private Timer KeepAliveTimer { get; set; }
	private Task MainTask { get; set; }

	public RaspberryClient(string serverHostname, int serverPort) {
		ServerHostname = serverHostname;
		ServerPort = serverPort;
		Client = new TcpClient();
		ApplicationToken = "xxxxxxx";
		KeepAliveTimer = new Timer(30 * 1000);
		KeepAliveTimer.Elapsed += KeepAlivePing;
	}

	public async Task<bool> Connect(int timeout = 15) {
		if (Client?.Connected is true)
			return false;

		TokenSource = new CancellationTokenSource();

		using var timer = new Timer(timeout * 1000);

		timer.Elapsed += (s, e) => TokenSource.Cancel();

		try {
			timer.Start();
			await Client.ConnectAsync(ServerHostname, ServerPort, TokenSource.Token);

			if (!await InitializeCommunicationAsync())
				return false;

			KeepAliveTimer.Start();
			MainTask = Task.Run(() => DataQueueLoop(TokenSource.Token), TokenSource.Token);
		}
		catch (Exception ex) {
			Console.WriteLine(ex.ToString());
			return false;
		}

		return true;
	}

	private async Task<bool> InitializeCommunicationAsync() {
		Client.ReceiveTimeout = 15000;
		Client.SendTimeout = 15000;

		using RSA rsa = RSA.Create(KeySizes.RSA_KEY_SIZE);

		// Send client public key
		if (!await TcpUtils.SendUnencryptedAsync(Client, rsa.ExportRSAPublicKey(), TokenSource.Token))
			return false;

		// Receive AES key and IV with RSA
		byte[] key = await TcpUtils.ReceiveUnencryptedAsync(Client, TokenSource.Token);
		if (key is null || key.Length < KeySizes.AES_KEY_SIZE / 8)
			return false;

		byte[] iv = await TcpUtils.ReceiveUnencryptedAsync(Client, TokenSource.Token);
		if (iv is null || iv.Length == 0)
			return false;

		key = rsa.Decrypt(key, RSAEncryptionPadding.OaepSHA512);
		iv = rsa.Decrypt(iv, RSAEncryptionPadding.OaepSHA512);

		ServerAes = CryptoUtils.CreateAes(key, iv);

		// Send secret application token
		return await TcpUtils.SendAsync(Client, ServerAes, Encoding.ASCII.GetBytes(ApplicationToken), TokenSource.Token);
	}

	private async void KeepAlivePing(object sender, EventArgs e) {
		bool pingSent = await TcpUtils.SendAsync(Client, ServerAes, Encoding.ASCII.GetBytes("ping"), TokenSource.Token);

		if (!pingSent && !TokenSource.Token.IsCancellationRequested) {
			KeepAliveTimer.Enabled = false;
			Disconnect();
		}
	}

	private async void DataQueueLoop(CancellationToken token) {
		while (!token.IsCancellationRequested) {
			byte[] data = await TcpUtils.ReceiveAsync(Client, ServerAes, token);
			if (data is not null && data.Length > 0)
				DataQueue.Enqueue(data);
		}
	}

	public async void Disconnect() {
		TokenSource.Cancel();
		KeepAliveTimer.Stop();
		await MainTask;
		Client.Close();
		TokenSource.Dispose();
	}

	public void Dispose() {
		if (Client != null && Client.Connected)
			Disconnect();

		Client.Dispose();
		GC.SuppressFinalize(this);
	}
}

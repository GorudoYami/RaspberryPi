using GorudoYami.Common.Attributes;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Timer = System.Timers.Timer;

namespace RaspberryPi.Modules;

[WorkInProgress]
[Obsolete("Tragiczny kod do poprawy")]
public class TcpClientModule : IDisposable {
	public string ServerHostname { get; set; }
	public int ServerPort { get; set; }

	private TcpClient Server { get; set; }
	private CancellationTokenSource TokenSource { get; set; }
	private Aes ServerAes { get; set; }
	private string ApplicationToken { get; set; }
	private Timer KeepAliveTimer { get; set; }
	private Task MainTask { get; set; }
	private ConcurrentQueue<byte[]> ReceiveDataQueue { get; set; }
	private ConcurrentQueue<byte[]> SendDataQueue { get; set; }

	public TcpClientModule(string serverHostname, int serverPort) {
		ServerHostname = serverHostname;
		ServerPort = serverPort;
		Server = new TcpClient();
		ApplicationToken = "xxxxxxx";
		KeepAliveTimer = new Timer(30 * 1000);
		KeepAliveTimer.Elapsed += KeepAlivePing;
		DataQueue = new ConcurrentQueue<byte[]>();
	}

	public async Task<bool> Connect(int timeout = 15) {
		if (Server?.Connected is true)
			return false;

		TokenSource = new CancellationTokenSource();

		using var timer = new Timer(timeout * 1000);

		timer.Elapsed += (s, e) => TokenSource.Cancel();

		try {
			timer.Start();
			await Server.ConnectAsync(ServerHostname, ServerPort, TokenSource.Token);

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
		Server.ReceiveTimeout = 15000;
		Server.SendTimeout = 15000;

		using RSA rsa = RSA.Create(KeySizes.RSA_KEY_SIZE);

		// Send client public key
		if (!await TcpUtils.SendUnencryptedAsync(Server, rsa.ExportRSAPublicKey(), TokenSource.Token))
			return false;

		// Receive AES key and IV with RSA
		byte[] key = await TcpUtils.ReceiveUnencryptedAsync(Server, TokenSource.Token);
		if (key is null || key.Length < KeySizes.AES_KEY_SIZE / 8)
			return false;

		byte[] iv = await TcpUtils.ReceiveUnencryptedAsync(Server, TokenSource.Token);
		if (iv is null || iv.Length == 0)
			return false;

		key = rsa.Decrypt(key, RSAEncryptionPadding.OaepSHA512);
		iv = rsa.Decrypt(iv, RSAEncryptionPadding.OaepSHA512);

		ServerAes = CryptoUtils.CreateAes(key, iv);

		// Send secret application token
		return await TcpUtils.SendAsync(Server, ServerAes, Encoding.ASCII.GetBytes(ApplicationToken), TokenSource.Token);
	}

	private async void KeepAlivePing(object sender, EventArgs e) {
		bool pingSent = await TcpUtils.SendAsync(Server, ServerAes, Encoding.ASCII.GetBytes("ping"), TokenSource.Token);

		if (!pingSent && !TokenSource.Token.IsCancellationRequested) {
			KeepAliveTimer.Enabled = false;
			Disconnect();
		}
	}

	private async void DataQueueLoop(CancellationToken token) {
		while (!token.IsCancellationRequested) {
			byte[] data = await TcpUtils.ReceiveAsync(Server, ServerAes, token);
			if (data is not null && data.Length > 0)
				ReceiveDataQueue.Enqueue(data);

			if (SendDataQueue.TryDequeue(out data)) {
				bool result = await TcpUtils.SendAsync(Server, ServerAes, data, TokenSource.Token);
				if (!result)
					Disconnect();
			}
		}
	}

	public async void Disconnect() {
		TokenSource.Cancel();
		KeepAliveTimer.Stop();
		await MainTask;
		Server.Close();
		TokenSource.Dispose();
	}

	public void Dispose() {
		if (Server != null && Server.Connected)
			Disconnect();

		Server.Dispose();
		GC.SuppressFinalize(this);
	}
}

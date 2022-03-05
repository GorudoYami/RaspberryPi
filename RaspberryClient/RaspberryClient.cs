using System;
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

	private TcpClient Client { get; set; }
	private CancellationTokenSource TokenSource { get; set; }
	private Aes ServerAes { get; set; }
	private string ApplicationToken { get; set; }
	private Timer KeepAliveTimer { get; set; }

	public RaspberryClient(string serverHostname, int serverPort) {
		ServerHostname = serverHostname;
		ServerPort = serverPort;
		TokenSource = new CancellationTokenSource();
		Client = new TcpClient();
		ApplicationToken = "uwu";
		KeepAliveTimer = new Timer(10 * 1000);
		KeepAliveTimer.Elapsed += KeepAlivePing;
	}

	private void KeepAlivePing(object sender, EventArgs e) {

	}

	public async Task<bool> Connect(int timeout = 15) {
		using var timer = new Timer(timeout * 1000);

		timer.Elapsed += (s, e) => TokenSource.Cancel();

		try {
			timer.Start();
			await Client.ConnectAsync(ServerHostname, ServerPort, TokenSource.Token);

			return !TokenSource.IsCancellationRequested && await InitializeCommunicationAsync();
		}
		catch (Exception ex) {
			Console.WriteLine(ex.ToString());
			return false;
		}
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

	public void Disconnect() {
		Client.Close();
	}

	public void Dispose() {
		if (Client != null && Client.Connected)
			Disconnect();

		TokenSource.Dispose();
		GC.SuppressFinalize(this);
	}
}

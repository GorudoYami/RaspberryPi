using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using RaspberryPi.Common;
using KeySizes = RaspberryPi.Common.KeySizes;

namespace RaspberryPi;

public class RaspberryClient {
	public string ServerHostname { get; set; }
	public int ServerPort { get; set; }

	private TcpClient Client { get; set; }

	public RaspberryClient(string serverHostname, int serverPort) {
		ServerHostname = serverHostname;
		ServerPort = serverPort;

		Client = new TcpClient();
	}

	public async Task<bool> Connect(int timeout = 15) {
		using var tokenSource = new CancellationTokenSource();
		using var timer = new System.Timers.Timer(timeout * 1000);

		timer.Elapsed += (s, e) => tokenSource.Cancel();

		try {
			timer.Start();
			await Client.ConnectAsync(ServerHostname, ServerPort, tokenSource.Token);

			return !tokenSource.IsCancellationRequested && await InitializeCommunication();
		}
		catch (Exception ex) {
			Console.WriteLine(ex.ToString());
			return false;
		}
	}

	private async Task<bool> InitializeCommunication() {
		Client.ReceiveTimeout = 15;
		Client.SendTimeout = 15;

		using RSA rsa = RSA.Create(KeySizes.RSA_KEY_SIZE);

		// Receive server public key
		if (!Await)
	}
}

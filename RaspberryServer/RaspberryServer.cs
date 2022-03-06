using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using RaspberryPi.Common;
using KeySizes = RaspberryPi.Common.KeySizes;
using Timer = System.Timers.Timer;

namespace RaspberryPi {
	public class RaspberryServer : IDisposable {
		public string Hostname { get; set; }
		public IPAddress Address { get; set; }
		public int Port { get; set; }

		private Dictionary<TcpClient, Aes> Clients { get; set; }
		private string ApplicationToken { get; set; }
		private TcpListener Listener { get; set; }
		private CancellationTokenSource TokenSource { get; set; }
		private Task MainTask { get; set; }

		public RaspberryServer(string hostname, int port) {
			Clients = new Dictionary<TcpClient, Aes>();
			Hostname = hostname;
			Address = NetworkUtils.GetAddressFromHostname(hostname);
			Port = port;

			Listener = new TcpListener(Address, Port);
		}

		public bool Start() {
			try {
				Listener.Start();

				// Start listening loop
				TokenSource = new CancellationTokenSource();
				MainTask = Task.Run(() => ListenAsync(TokenSource.Token), TokenSource.Token);
			}
			catch (Exception ex) {
				Console.WriteLine(ex.ToString());
				return false;
			}

			return true;
		}

		public async void StopAsync() {
			if (MainTask.Status is not TaskStatus.Running)
				return;

			// Send cancel request and wait for the task
			TokenSource.Cancel();
			await MainTask;
			TokenSource.Dispose();

			Cleanup();
		}

		private void Cleanup() {
			foreach (TcpClient client in Clients.Keys)
				CleanupClient(client);

			Clients.Clear();
		}

		private static void CleanupClient(TcpClient client) {
			client.Close();
			client.Dispose();
		}

		private async void ListenAsync(CancellationToken token) {
			while (!token.IsCancellationRequested) {
				TcpClient client = await Listener.AcceptTcpClientAsync(token);

				if (client.Connected) {
					var aes = await InitializeCommunicationAsync(client);

					if (aes is not null)
						Clients.Add(client, aes);
					else
						CleanupClient(client);
				}
			}
		}

		private async Task<Aes> InitializeCommunicationAsync(TcpClient client) {
			client.ReceiveTimeout = 15000;
			client.SendTimeout = 15000;

			using RSA rsa = RSA.Create(KeySizes.RSA_KEY_SIZE);

			// Receive client public key
			byte[] buffer = await TcpUtils.ReceiveUnencryptedAsync(client, TokenSource.Token);
			if (buffer is null || buffer.Length < KeySizes.RSA_KEY_SIZE / 8)
				return null;

			// Import client public key
			rsa.ImportRSAPublicKey(buffer, out int bytesRead);
			if (bytesRead != buffer.Length)
				return null;

			Aes clientAes = CryptoUtils.CreateAes();

			// Send encrypted AES key and IV with RSA
			if (!await TcpUtils.SendUnencryptedAsync(client, rsa.Encrypt(clientAes.Key, RSAEncryptionPadding.OaepSHA512), TokenSource.Token))
				return null;

			if (!await TcpUtils.SendUnencryptedAsync(client, rsa.Encrypt(clientAes.IV, RSAEncryptionPadding.OaepSHA512), TokenSource.Token))
				return null;

			// Receive secret application token
			buffer = await TcpUtils.ReceiveAsync(client, Clients[client], TokenSource.Token);

			return Encoding.ASCII.GetString(buffer) != ApplicationToken ? null : clientAes;
		}

		public void Dispose() {
			StopAsync();
			GC.SuppressFinalize(this);
		}
	}
}
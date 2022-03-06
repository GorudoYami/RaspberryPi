using RaspberryPi.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace RaspberryPi.Common {
	public static class TcpUtils {
		public static async Task<bool> SendUnencryptedAsync(TcpClient client, byte[] data, CancellationToken token) {
			try {
				using var stream = client.GetStream();

				await stream.WriteAsync(data, token);
				await stream.WriteAsync(Encoding.ASCII.GetBytes("\r\n"), token);
			}
			catch (Exception ex) {
				Console.WriteLine(ex.ToString());
				return false;
			}

			return true;
		}

		public static async Task<byte[]> ReceiveUnencryptedAsync(TcpClient client, CancellationToken token) {
			string data = string.Empty;
			byte[] buffer = new byte[1024];

			bool timedOut = false;
			using var timer = new Timer(client.ReceiveTimeout * 1000);
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

			// Erase "\r\n" from data
			data = data[0..^2];
			return Encoding.ASCII.GetBytes(data);
		}

		public static async Task<bool> SendAsync(TcpClient client, Aes aes, byte[] data, CancellationToken token) {
			try {
				using var stream = client.GetStream();

				await stream.WriteAsync(CryptoUtils.EncryptData(data, aes), token);
				await stream.WriteAsync(Encoding.ASCII.GetBytes("\r\n"), token);
			}
			catch (Exception ex) {
				Console.WriteLine(ex.ToString());
				return false;
			}

			return true;
		}

		public static async Task<byte[]> ReceiveAsync(TcpClient client, Aes aes, CancellationToken token) {
			string data = string.Empty;
			byte[] buffer = new byte[1024];

			bool timedOut = false;
			using var timer = new Timer(client.ReceiveTimeout * 1000);
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
	}
}
using System.Net.Sockets;
using System.Text;

namespace RaspberryPi.TcpServerModule.Extensions;

public static class TcpClientExtensions {


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

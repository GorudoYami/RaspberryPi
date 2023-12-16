using GorudoYami.Common.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RaspberryPi.TcpServerModule.Extensions;

public static class TcpClientExtensions {
	public static async Task<bool> SendAsync(
		this TcpClient client,
		byte[] data,
		bool encrypt = true,
		CancellationToken cancellationToken = default) {

		try {
			ICryptographyService cryptographyService = new CryptographyService();
			using var stream = client.GetStream();
			await stream.WriteAsync(cryptographyService.Encrypt(data, cancellationToken: cancellationToken), cancellationToken);
			await stream.WriteAsync(Encoding.ASCII.GetBytes("\r\n"), cancellationToken);
		}
		catch (Exception ex) {
			Console.WriteLine(ex.ToString());
			return false;
		}

		return true;
	}
}

using GorudoYami.Common.Cryptography;
using System.Net.Sockets;

namespace RaspberryPi.Modules.Models;

public class TcpClientInfo : IDisposable, IAsyncDisposable {
	public NetworkStream NetworkStream => TcpClient.GetStream();
	public TcpClient TcpClient { get; }
	public CryptoStreamReaderWriter IO { get; }

	public TcpClientInfo(TcpClient client, CryptoStreamReaderWriter io) {
		TcpClient = client;
		IO = io;
	}

	public void Dispose() {
		GC.SuppressFinalize(this);

		IO.Dispose();
		TcpClient.Dispose();
	}

	public async ValueTask DisposeAsync() {
		GC.SuppressFinalize(this);

		await IO.DisposeAsync();
		TcpClient.Dispose();
	}
}

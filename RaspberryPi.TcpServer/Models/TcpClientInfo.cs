using GorudoYami.Common.Cryptography;
using System.Net.Sockets;

namespace RaspberryPi.TcpServer.Models;

public class TcpClientInfo : IDisposable, IAsyncDisposable {
	public TcpClient TcpClient { get; }
	public CryptoStreamReaderWriter ReaderWriter { get; }
	public NetworkStream Stream => TcpClient.GetStream();

	public TcpClientInfo(TcpClient client, CryptoStreamReaderWriter readerWriter) {
		TcpClient = client;
		ReaderWriter = readerWriter;
	}

	public void Dispose() {
		GC.SuppressFinalize(this);

		ReaderWriter.Dispose();
		TcpClient.Dispose();
	}

	public async ValueTask DisposeAsync() {
		GC.SuppressFinalize(this);

		await ReaderWriter.DisposeAsync();
		TcpClient.Dispose();
	}
}

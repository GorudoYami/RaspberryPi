using GorudoYami.Common.Streams;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RaspberryPi.Server.Models {
	public class TcpClientInfo : IDisposable, IAsyncDisposable {
		public TcpClient MainTcpClient { get; }
		public TcpClient VideoTcpClient { get; private set; }
		public NetworkStream MainStream => MainTcpClient.GetStream();
		public NetworkStream VideoStream => VideoTcpClient?.GetStream();

		public ByteStreamReaderWriter MainReaderWriter { get; }
		public ByteStreamReaderWriter VideoReaderWriter { get; private set; }

		public TcpClientInfo(TcpClient client, string delimiter) {
			MainTcpClient = client;
			MainReaderWriter = new ByteStreamReaderWriter(MainStream, firstDelimiter: delimiter[0], secondDelimiter: GetSecondCharacter(delimiter));
		}

		private char? GetSecondCharacter(string delimiter) {
			if (delimiter.Length > 1) {
				return delimiter[1];
			}

			return null;
		}

		public void SetVideoClient(TcpClient client) {
			VideoTcpClient = client;
			VideoReaderWriter = new ByteStreamReaderWriter(VideoStream);
		}

		public void Dispose() {
			GC.SuppressFinalize(this);

			MainTcpClient.Dispose();
		}

		public async ValueTask DisposeAsync() {
			GC.SuppressFinalize(this);
			MainTcpClient.Dispose();
			await Task.CompletedTask;
		}
	}
}

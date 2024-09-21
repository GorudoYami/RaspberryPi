using RaspberryPi.Common.Utilities;
using System;
using System.Net.Sockets;

namespace RaspberryPi.TcpServer.Models {
	public class TcpClientInfo : IDisposable {
		public TcpClient Client { get; }
		public NetworkStream Stream => Client.GetStream();

		public ByteStreamReaderWriter IO { get; }

		public TcpClientInfo(TcpClient client, string delimiter) {
			Client = client;
			IO = new ByteStreamReaderWriter(Stream, firstDelimiter: delimiter[0], secondDelimiter: GetSecondCharacter(delimiter));
		}

		private static char? GetSecondCharacter(string delimiter) {
			if (delimiter.Length > 1) {
				return delimiter[1];
			}

			return null;
		}

		public void Dispose() {
			GC.SuppressFinalize(this);

			Client.Dispose();
		}
	}
}

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Utilities {
	public class ByteStreamReaderWriter : IDisposable {
		private readonly ByteStreamReader _reader;
		private readonly ByteStreamWriter _writer;

		public ByteStreamReaderWriter(Stream stream, bool leaveOpen = false, char firstDelimiter = '\r', char? secondDelimiter = '\n') {
			_reader = new ByteStreamReader(stream, leaveOpen, firstDelimiter, secondDelimiter);
			_writer = new ByteStreamWriter(stream, leaveOpen, firstDelimiter, secondDelimiter);
		}

		public async Task<byte[]> ReadMessageAsync(CancellationToken cancellationToken = default) {
			return await _reader.ReadMessageAsync(cancellationToken);
		}

		public async Task WriteMessageAsync(byte[] message, CancellationToken cancellationToken = default) {
			await _writer.WriteMessageAsync(message, cancellationToken);
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			_reader.Dispose();
			_writer.Dispose();
		}
	}
}

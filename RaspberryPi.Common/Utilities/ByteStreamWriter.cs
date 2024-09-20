using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Utilities {
	public class ByteStreamWriter : IDisposable, IAsyncDisposable {
		public char FirstDelimiter { get; set; }
		public char? SecondDelimiter { get; set; }
		private readonly Stream _stream;
		private readonly bool _leaveOpen;

		public ByteStreamWriter(Stream stream, bool leaveOpen = false, char firstDelimiter = '\r', char? secondDelimiter = '\n') {
			FirstDelimiter = firstDelimiter;
			SecondDelimiter = secondDelimiter;
			_stream = stream;
			_leaveOpen = leaveOpen;
		}

		public async Task WriteMessageAsync(byte[] data, CancellationToken cancellationToken = default) {
			await _stream.WriteAsync(data, 0, data.Length, cancellationToken);
			_stream.WriteByte((byte)FirstDelimiter);
			if (SecondDelimiter != null) {
				_stream.WriteByte((byte)SecondDelimiter);
			}
		}

		public void Dispose() {
			GC.SuppressFinalize(this);

			if (_leaveOpen == false) {
				_stream.Dispose();
			}
		}

		public async ValueTask DisposeAsync() {
			GC.SuppressFinalize(this);
			await Task.Run(() => {
				if (_leaveOpen == false) {
					_stream.Dispose();
				}
			});
		}
	}
}

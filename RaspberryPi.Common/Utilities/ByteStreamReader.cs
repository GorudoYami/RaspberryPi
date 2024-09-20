using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Utilities {
	public class ByteStreamReader : IDisposable {
		public char FirstDelimiter { get; set; }
		public char? SecondDelimiter { get; set; }

		private readonly Stream _stream;
		private readonly bool _leaveOpen;

		public ByteStreamReader(Stream stream, bool leaveOpen = false, char firstDelimiter = '\r', char? secondDelimiter = '\n') {
			FirstDelimiter = firstDelimiter;
			SecondDelimiter = secondDelimiter;
			_stream = stream;
			_leaveOpen = leaveOpen;
		}

		public async Task<byte[]> ReadMessageAsync(CancellationToken cancellationToken = default) {
			using (var ms = new MemoryStream()) {
				int byteRead;

				while ((byteRead = _stream.ReadByte()) != -1) {
					if (byteRead == FirstDelimiter && SecondDelimiter == null) {
						break;
					}
					else if (byteRead == FirstDelimiter && SecondDelimiter != null) {
						int nextByte = _stream.ReadByte();

						if (nextByte == SecondDelimiter) {
							break;
						}

						ms.WriteByte((byte)byteRead);
						ms.WriteByte((byte)nextByte);
					}
					else {
						ms.WriteByte((byte)byteRead);
					}
				}

				await ms.FlushAsync(cancellationToken);
				return ms.ToArray();
			}
		}

		public void Dispose() {
			GC.SuppressFinalize(this);

			if (_leaveOpen == false) {
				_stream.Dispose();
			}
		}
	}
}

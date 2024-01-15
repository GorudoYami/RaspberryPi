using GorudoYami.Common.Cryptography;
using GorudoYami.Common.Streams;
using RaspberryPi.Common.Exceptions;
using RaspberryPi.Common.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Protocols {
	public class EncryptedClientProtocol : IClientProtocol {
		public event EventHandler<MessageReceivedEventArgs> MessageReceived;

		private readonly List<byte> _messageTypes;
		private Aes _serverAes;
		private CryptoStreamReaderWriter _serverReaderWriter;

		public EncryptedClientProtocol() {
			_messageTypes = Enum.GetValues(typeof(MessageType)).Cast<byte>().ToList();
		}

		public async Task<Stream> InitializeCommunicationAsync(Stream serverStream, CancellationToken cancellationToken = default) {
			ByteStreamReader serverReader = null;
			RSA rsa = null;
			try {
				serverReader = new ByteStreamReader(serverStream, true);
				rsa = RSA.Create();
				rsa.KeySize = CryptographyKeySizes.RsaKeySizeBits;
				byte[] data = JsonSerializer.SerializeToUtf8Bytes(rsa.ExportParameters(false));
				await serverStream.WriteAsync(data, 0, data.Length, cancellationToken);
				data = Encoding.ASCII.GetBytes("\r\n");
				await serverStream.WriteAsync(data, 0, data.Length, cancellationToken);

				bool result = false;
				try {
					_serverAes = Aes.Create();
					_serverAes.KeySize = CryptographyKeySizes.AesKeySizeBits;

					data = await serverReader.ReadMessageAsync(cancellationToken);
					int expectedLength = CryptographyKeySizes.AesKeySizeBits / 8;
					if (data.Length != expectedLength) {
						throw new InitializeCommunicationException($"Received AES key has an invalid size. Expected {expectedLength}. Actual: {data.Length}");
					}
					_serverAes.Key = rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA512);

					data = await serverReader.ReadMessageAsync(cancellationToken);
					expectedLength = CryptographyKeySizes.AesIvSizeBits / 8;
					if (data.Length != expectedLength) {
						throw new InitializeCommunicationException($"Received AES IV has an invalid size. Expected {expectedLength}. Actual: {data.Length}");
					}
					_serverAes.IV = rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA512);

					_serverReaderWriter = new CryptoStreamReaderWriter(_serverAes.CreateEncryptor(), _serverAes.CreateDecryptor(), serverStream);
					await _serverReaderWriter.WriteLineAsync("OK", cancellationToken);

					result = true;
					return _serverReaderWriter;
				}
				finally {
					await CleanupAsync(result);
				}
			}
			finally {
				rsa?.Dispose();
			}
		}

		private async Task CleanupAsync(bool result) {
			if (result == false) {
				if (_serverReaderWriter != null) {
					await _serverReaderWriter.DisposeAsync();
				}
			}

			_serverAes = null;
			_serverReaderWriter = null;
		}

		public void ParseMessage(byte[] message) {
			if (message.Length == 0) {
				throw new ProtocolException("Message to parse was empty");
			}
			else if (message.Length > 2) {
				throw new ProtocolException("Message was too big to parse");
			}

			MessageType messageType = MessageType.Unknown;
			byte messageValue = message.Length == 2 ? message[1] : (byte)0;
			if (_messageTypes.Any(x => x == message[0])) {
				messageType = (MessageType)message[0];
			}

			MessageReceived?.Invoke(this, new MessageReceivedEventArgs(messageType, messageValue));
		}
	}
}

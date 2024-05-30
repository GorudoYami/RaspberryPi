using GorudoYami.Common.Cryptography;
using GorudoYami.Common.Streams;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
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
		public string Delimiter => "\r\n";
		public event EventHandler<MessageReceivedEventArgs> MessageReceived;

		private readonly List<byte> _messageTypes;
		private Aes _clientAes;
		private CryptoStreamReaderWriter _serverReaderWriter;
		private Stream _serverStream;
		private ByteStreamReader _serverReader;

		public EncryptedClientProtocol() {
			_messageTypes = Enum.GetValues(typeof(MessageType)).Cast<byte>().ToList();
		}

		public async Task<Stream> InitializeCommunicationAsync(Stream serverStream, CancellationToken cancellationToken = default) {
			_serverStream = serverStream;

			try {
				_serverReader = new ByteStreamReader(serverStream, true);

				bool result = false;
				try {
					_clientAes = CreateAes();
					AsymmetricCipherKeyPair rsaKeyPair = CreateRsa();
					await SendPublicKeyAsync(rsaKeyPair, cancellationToken);
					await Read
					byte[] data = await _serverReader.ReadMessageAsync(cancellationToken);
					int expectedLength = CryptographyKeySizes.AesKeySizeBits / 8;
					if (data.Length != expectedLength) {
						throw new InitializeCommunicationException($"Received AES key has an invalid size. Expected {expectedLength}. Actual: {data.Length}");
					}
					_clientAes.Key = rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA512);

					data = await serverReader.ReadMessageAsync(cancellationToken);
					expectedLength = CryptographyKeySizes.AesIvSizeBits / 8;
					if (data.Length != expectedLength) {
						throw new InitializeCommunicationException($"Received AES IV has an invalid size. Expected {expectedLength}. Actual: {data.Length}");
					}
					_clientAes.IV = rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA512);

					_serverReaderWriter = new CryptoStreamReaderWriter(_clientAes.CreateEncryptor(), _clientAes.CreateDecryptor(), serverStream);
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
				serverReader?.Dispose();
			}
		}

		private static Aes CreateAes() {
			var aes = Aes.Create();
			aes.KeySize = CryptographyKeySizes.AesKeySizeBits;
			aes.Key = new byte[CryptographyKeySizes.AesKeySizeBits / 8];
			aes.IV = new byte[CryptographyKeySizes.AesIvSizeBits / 8];

			using (var rng = RandomNumberGenerator.Create()) {
				rng.GetBytes(aes.Key);
				rng.GetBytes(aes.IV);
			}

			return aes;
		}

		private static AsymmetricCipherKeyPair CreateRsa() {
			var generator = new RsaKeyPairGenerator();
			generator.Init(new KeyGenerationParameters(new SecureRandom(), CryptographyKeySizes.RsaKeySizeBits));
			AsymmetricCipherKeyPair keyPair = generator.GenerateKeyPair();
			return keyPair;
		}

		private async Task SendPublicKeyAsync(AsymmetricCipherKeyPair rsaKeyPair, CancellationToken cancellationToken) {

			var publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(rsaKeyPair.Public);
			byte[] data = publicKeyInfo.GetDerEncoded();
			await _serverStream.WriteAsync(data, 0, data.Length, cancellationToken);
			data = Encoding.ASCII.GetBytes(Delimiter);
			await _serverStream.WriteAsync(data, 0, data.Length, cancellationToken);
		}

		private async Task CleanupAsync(bool result) {
			if (result == false) {
				if (_serverReaderWriter != null) {
					await _serverReaderWriter.DisposeAsync();
				}
			}

			_clientAes = null;
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

		public Aes GetLastUsedAes() {
			return _clientAes;
		}
	}
}

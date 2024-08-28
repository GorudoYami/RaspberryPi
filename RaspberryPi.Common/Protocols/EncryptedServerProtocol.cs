using GorudoYami.Common.Cryptography;
using GorudoYami.Common.Streams;
using Microsoft.Extensions.Logging;
using RaspberryPi.Common.Exceptions;
using RaspberryPi.Common.Utilities;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Protocols {
	public class EncryptedServerProtocol : IServerProtocol {
		public string Delimiter => "\r\n";

		private Aes _aes;
		private CryptoStreamReaderWriter _clientReaderWriter;

		public async Task<Stream> InitializeCommunicationAsync(Stream clientStream, CancellationToken cancellationToken = default) {
			ByteStreamReader clientReader = null;
			RSA rsa = null;

			try {
				clientReader = new ByteStreamReader(clientStream, true);
				rsa = RSA.Create();
				rsa.KeySize = CryptographyKeySizes.RsaKeySizeBits;

				byte[] data = await clientReader.ReadMessageAsync(cancellationToken: cancellationToken);
				int expectedLength = CryptographyKeySizes.RsaKeySizeBits / 8 + CryptographyKeySizes.RsaKeyInfoSizeBits / 8;
				if (data.Length != expectedLength) {
					throw new InitializeCommunicationException($"Received public key has an invalid size. Expected: {expectedLength}. Actual: {data.Length}.");
				}

				RSAParameters rsaParameters = JsonSerializer.Deserialize<RSAParameters>(data);
				rsa.ImportParameters(rsaParameters);

				bool result = false;
				try {
					_aes = CreateAes();

					data = rsa.Encrypt(_aes.Key, RSAEncryptionPadding.OaepSHA512);
					await clientStream.WriteAsync(data, 0, data.Length, cancellationToken);
					data = Encoding.ASCII.GetBytes(Delimiter);
					await clientStream.WriteAsync(data, 0, data.Length, cancellationToken);

					data = rsa.Encrypt(_aes.IV, RSAEncryptionPadding.OaepSHA512);
					await clientStream.WriteAsync(data, 0, data.Length, cancellationToken);
					data = Encoding.ASCII.GetBytes(Delimiter);
					await clientStream.WriteAsync(data, 0, data.Length, cancellationToken);

					_clientReaderWriter = new CryptoStreamReaderWriter(_aes.CreateEncryptor(), _aes.CreateDecryptor(), clientStream);
					string response = await _clientReaderWriter.ReadLineAsync(cancellationToken);
					if (response == "OK") {
						result = true;
						return _clientReaderWriter;
					}
					else {
						throw new InitializeCommunicationException($"Did not receive correct response. Expected: OK. Received: {response}");
					}
				}
				finally {
					await CleanupAsync(result);
				}
			}
			finally {
				rsa?.Dispose();
				clientReader?.Dispose();
			}
		}

		private async Task CleanupAsync(bool result) {
			if (result == false) {
				_aes?.Dispose();

				if (_clientReaderWriter != null) {
					await _clientReaderWriter.DisposeAsync();
				}
			}

			_clientReaderWriter = null;
			_aes = null;
		}

		public Aes GetLastUsedAes() {
			return _aes;
		}
	}
}

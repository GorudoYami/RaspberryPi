using GorudoYami.Common.Cryptography;
using GorudoYami.Common.Streams;
using RaspberryPi.Common.Exceptions;
using RaspberryPi.Common.Utilities;
using System.Security.Cryptography;
using System.Text;

namespace RaspberryPi.Common.Protocols;

public class EncryptedServerProtocol : IServerProtocol {
	private Aes? _aes;
	private CryptoStreamReaderWriter? _clientReaderWriter;

	public async Task<Stream> InitializeCommunicationAsync(Stream clientStream, CancellationToken cancellationToken = default) {
		using var clientReader = new ByteStreamReader(clientStream, true);
		using var rsa = RSA.Create(CryptographyKeySizes.RsaKeySizeBits);

		byte[] data = await clientReader.ReadMessageAsync(cancellationToken: cancellationToken);
		int expectedLength = CryptographyKeySizes.RsaKeySizeBits / 8 + CryptographyKeySizes.RsaKeyInfoSizeBits / 8;
		if (data.Length != expectedLength) {
			throw new InitializeCommunicationException($"Received public key has an invalid size. Expected: {expectedLength}. Actual: {data.Length}.");
		}

		rsa.ImportRSAPublicKey(data, out int bytesRead);
		if (bytesRead != data.Length) {
			throw new InitializeCommunicationException($"Public key has not been read fully. Expected: {data.Length}. Read: {bytesRead}.");
		}

		bool result = false;
		try {
			_aes = CreateAes();

			data = rsa.Encrypt(_aes.Key, RSAEncryptionPadding.OaepSHA512);
			await clientStream.WriteAsync(data, cancellationToken);
			await clientStream.WriteAsync(Encoding.ASCII.GetBytes("\r\n"), cancellationToken);

			data = rsa.Encrypt(_aes.IV, RSAEncryptionPadding.OaepSHA512);
			await clientStream.WriteAsync(data, cancellationToken);
			await clientStream.WriteAsync(Encoding.ASCII.GetBytes("\r\n"), cancellationToken);

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

	private static Aes CreateAes() {
		var aes = Aes.Create();
		aes.KeySize = CryptographyKeySizes.AesKeySizeBits;
		aes.Key = RandomNumberGenerator.GetBytes(CryptographyKeySizes.AesKeySizeBits / 8);
		aes.IV = RandomNumberGenerator.GetBytes(CryptographyKeySizes.AesIvSizeBits / 8);
		return aes;
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
}

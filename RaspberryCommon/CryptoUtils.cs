using System.Security.Cryptography;

namespace RaspberryPi.Common;

public static class CryptoUtils {
	public static byte[] DecryptData(byte[] data, Aes aes) {
		// Calculate loop count and create output buffer
		int blockSize = aes.BlockSize;
		int loops = data.Length / blockSize;
		byte[] result = new byte[data.Length];

		using var decryptor = aes.CreateDecryptor();

		// Decrypt data
		for (int i = 0; i < loops; i++)
			decryptor.TransformBlock(data, i * blockSize, blockSize, result, i * blockSize);

		// Decrypt last block
		if (data.Length % blockSize != 0)
			decryptor.TransformFinalBlock(data, loops * blockSize, data.Length % blockSize);

		return result;
	}

	public static byte[] EncryptData(byte[] data, Aes aes) {
		// Calculate loop count and create output buffer
		int blockSize = aes.BlockSize;
		int loops = data.Length / blockSize;
		byte[] result = new byte[data.Length];

		using var encryptor = aes.CreateEncryptor();

		// Encrypt blocks
		for (int i = 0; i < loops; i++)
			encryptor.TransformBlock(data, i * blockSize, blockSize, result, i * blockSize);

		// Encrypt last block
		if (data.Length % blockSize != 0)
			encryptor.TransformFinalBlock(data, loops * blockSize, data.Length % blockSize);

		return result;
	}

	public static Aes GenerateAes() {
		Aes aes = Aes.Create();
		aes.KeySize = KeySizes.AES_KEY_SIZE;
		aes.GenerateKey();
		aes.GenerateIV();
		return aes;
	}
}

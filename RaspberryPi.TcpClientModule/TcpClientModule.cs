using GorudoYami.Common.Cryptography;
using GorudoYami.Common.Streams;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Utilities;
using RaspberryPi.Modules.Models;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace RaspberryPi.Modules;

public class TcpClientModule : ITcpClientModule, IDisposable, IAsyncDisposable {
	private readonly TcpClientModuleOptions _options;

	private TcpClient? _server;
	private Aes? _serverAes;
	private CryptoStreamReaderWriter? _serverReaderWriter;
	private ByteStreamReader? _serverUnencryptedReader;

	public TcpClientModule(IOptions<TcpClientModuleOptions> options) {
		_options = options.Value;
		_server = new TcpClient() {
			ReceiveTimeout = _options.TimeoutSeconds * 1000,
			SendTimeout = _options.TimeoutSeconds * 1000,
		};
	}

	public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default) {
		if (_server?.Connected == true) {
			await DisconnectAsync();
		}

		_server = new TcpClient() {
			ReceiveTimeout = _options.TimeoutSeconds * 1000,
			SendTimeout = _options.TimeoutSeconds * 1000,
		};

		Task timeoutTask = Task.Delay(_options.TimeoutSeconds, cancellationToken);
		Task connectTask = _server.ConnectAsync(_options.ServerHost, _options.ServerPort);
		await Task.WhenAny(timeoutTask, connectTask);

		return _server.Connected && await InitializeCommunicationAsync(cancellationToken);
	}

	private async Task<bool> InitializeCommunicationAsync(CancellationToken cancellationToken = default) {
		var serverStream = _server!.GetStream();
		_serverUnencryptedReader = new ByteStreamReader(serverStream, true);

		using RSA rsa = RSA.Create(CryptographyKeySizes.RsaKeySizeBits);

		await serverStream.WriteAsync(rsa.ExportRSAPublicKey(), cancellationToken);
		await serverStream.WriteAsync(Encoding.ASCII.GetBytes("\r\n"), cancellationToken);

		bool result = false;
		try {
			_serverAes = Aes.Create();
			_serverAes.KeySize = CryptographyKeySizes.AesKeySizeBits;

			byte[] data = await _serverUnencryptedReader.ReadMessageAsync(cancellationToken);
			if (data.Length != CryptographyKeySizes.AesKeySizeBits / 8) {
				return false;
			}
			_serverAes.Key = rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA512);

			data = await _serverUnencryptedReader.ReadMessageAsync(cancellationToken);
			if (data.Length != CryptographyKeySizes.AesIvSizeBits / 8) {
				return false;
			}
			_serverAes.IV = rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA512);

			_serverReaderWriter = new CryptoStreamReaderWriter(_serverAes.CreateEncryptor(), _serverAes.CreateDecryptor(), serverStream);
			await _serverReaderWriter.WriteLineAsync("OK", cancellationToken);

			result = true;
			return result;
		}
		finally {
			if (result == false) {
				await DisconnectAsync();
			}
		}
	}

	public async Task SendAsync(
		byte[] data,
		CancellationToken cancellationToken = default) {
		AssertConnected();
		await _serverReaderWriter!.WriteMessageAsync(data, cancellationToken);
	}

	public async Task SendAsync(
		string data,
		CancellationToken cancellationToken = default) {
		AssertConnected();
		await _serverReaderWriter!.WriteLineAsync(data, cancellationToken);
	}

	public async Task ReadLineAsync(CancellationToken cancellationToken = default) {
		AssertConnected();
		await _serverReaderWriter!.ReadLineAsync(cancellationToken);
	}

	public async Task ReadAsync(CancellationToken cancellationToken = default) {
		AssertConnected();
		await _serverReaderWriter!.ReadMessageAsync(cancellationToken);
	}

	private void AssertConnected() {
		if (_server == null) {
			throw new InvalidOperationException("Not connected to a server");
		}
	}

	public async Task DisconnectAsync() {
		if (_serverUnencryptedReader != null) {
			await _serverUnencryptedReader.DisposeAsync();
			_serverUnencryptedReader = null;
		}

		if (_serverReaderWriter != null) {
			await _serverReaderWriter.DisposeAsync();
			_serverReaderWriter = null;
		}

		_serverAes?.Dispose();
		_serverAes = null;
		_server?.Dispose();
		_server = null;
	}

	public void Dispose() {
		GC.SuppressFinalize(this);
		DisconnectAsync().GetAwaiter().GetResult();
	}

	public async ValueTask DisposeAsync() {
		GC.SuppressFinalize(this);
		await DisconnectAsync();
	}
}

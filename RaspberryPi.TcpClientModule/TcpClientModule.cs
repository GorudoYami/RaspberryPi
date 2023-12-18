using GorudoYami.Common.Asynchronous;
using GorudoYami.Common.Cryptography;
using GorudoYami.Common.Modules;
using GorudoYami.Common.Streams;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Utilities;
using RaspberryPi.Modules.Models;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace RaspberryPi.Modules;

public interface ITcpClientModule : IModule, IDisposable, IAsyncDisposable {
	Task<bool> ConnectAsync();
	Task DisconnectAsync();
	Task ReadAsync(CancellationToken cancellationToken = default);
	Task ReadLineAsync(CancellationToken cancellationToken = default);
	Task SendAsync(byte[] data, CancellationToken cancellationToken = default);
	Task SendAsync(string data, CancellationToken cancellationToken = default);
}

public class TcpClientModule : ITcpClientModule {
	private CancellationToken Token => _cancellationTokenProvider.GetToken();
	private readonly ICancellationTokenProvider _cancellationTokenProvider;
	private readonly TcpClientOptions _options;

	private TcpClient? _server;
	private Aes? _serverAes;
	private CryptoStreamReaderWriter? _serverReaderWriter;
	private ByteStreamReader? _serverUnencryptedReader;

	public TcpClientModule(IOptions<TcpClientOptions> options, ICancellationTokenProvider cancellationTokenProvider) {
		_options = options.Value;
		_cancellationTokenProvider = cancellationTokenProvider;
		_server = new TcpClient() {
			ReceiveTimeout = _options.TimeoutSeconds * 1000,
			SendTimeout = _options.TimeoutSeconds * 1000,
		};
	}

	public async Task<bool> ConnectAsync() {
		if (_server?.Connected == true) {
			await DisconnectAsync();
		}

		_server = new TcpClient() {
			ReceiveTimeout = _options.TimeoutSeconds * 1000,
			SendTimeout = _options.TimeoutSeconds * 1000,
		};

		Task timeoutTask = Task.Delay(_options.TimeoutSeconds, Token);
		Task connectTask = _server.ConnectAsync(_options.ServerHost, _options.ServerPort);
		await Task.WhenAny(timeoutTask, connectTask);

		return _server.Connected && await InitializeCommunicationAsync();
	}

	private async Task<bool> InitializeCommunicationAsync() {
		var serverStream = _server!.GetStream();
		_serverUnencryptedReader = new ByteStreamReader(serverStream, true);

		using RSA rsa = RSA.Create(CryptographyKeySizes.RsaKeySizeBits);

		await serverStream.WriteAsync(rsa.ExportRSAPublicKey(), Token);
		await serverStream.WriteAsync(Encoding.ASCII.GetBytes("\r\n"), Token);

		bool result = false;
		try {
			_serverAes = Aes.Create();
			_serverAes.KeySize = CryptographyKeySizes.AesKeySizeBits;

			byte[] data = await _serverUnencryptedReader.ReadMessageAsync(Token);
			if (data.Length != CryptographyKeySizes.AesKeySizeBits / 8) {
				return false;
			}
			_serverAes.Key = rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA512);

			data = await _serverUnencryptedReader.ReadMessageAsync(Token);
			if (data.Length != CryptographyKeySizes.AesIvSizeBits / 8) {
				return false;
			}
			_serverAes.IV = rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA512);

			_serverReaderWriter = new CryptoStreamReaderWriter(_serverAes.CreateEncryptor(), _serverAes.CreateDecryptor(), serverStream);
			await _serverReaderWriter.WriteLineAsync("OK", Token);

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

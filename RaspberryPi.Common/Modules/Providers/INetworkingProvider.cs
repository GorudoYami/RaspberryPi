using GorudoYami.Common.Modules;
using System.Net;

namespace RaspberryPi.Common.Modules.Providers;

public interface INetworkingProvider : IModule {
	public IPAddress ServerAddress { get; }
	public bool Connected { get; }

	public Task ConnectAsync(CancellationToken cancellationToken = default);
	public Task DisconnectAsync(CancellationToken cancellationToken = default);
	Task<byte[]> ReadAsync(CancellationToken cancellationToken = default);
	Task<string> ReadLineAsync(CancellationToken cancellationToken = default);
	Task SendAsync(byte[] data, CancellationToken cancellationToken = default);
	Task SendAsync(string data, CancellationToken cancellationToken = default);
}

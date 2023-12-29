using GorudoYami.Common.Modules;
using System.Net;

namespace RaspberryPi.Common.Modules;

public interface INetworkModule : IModule {
	public IPAddress ServerAddress { get; }
	public bool Connected { get; }

	public Task ConnectAsync(CancellationToken cancellationToken = default);
	public Task DisconnectAsync(CancellationToken cancellationToken = default);
}

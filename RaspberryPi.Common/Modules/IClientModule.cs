using GorudoYami.Common.Modules;
using System.Net;

namespace RaspberryPi.Common.Modules;

public interface IClientModule : INetworkModule {
	Task ConnectAsync(CancellationToken cancellationToken = default);
	Task DisconnectAsync();
	Task ReadAsync(CancellationToken cancellationToken = default);
	Task ReadLineAsync(CancellationToken cancellationToken = default);
	Task SendAsync(byte[] data, CancellationToken cancellationToken = default);
	Task SendAsync(string data, CancellationToken cancellationToken = default);
}
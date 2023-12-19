using GorudoYami.Common.Modules;

namespace RaspberryPi.Common.Modules;

public interface ITcpClientModule : IModule {
	Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
	Task DisconnectAsync();
	Task ReadAsync(CancellationToken cancellationToken = default);
	Task ReadLineAsync(CancellationToken cancellationToken = default);
	Task SendAsync(byte[] data, CancellationToken cancellationToken = default);
	Task SendAsync(string data, CancellationToken cancellationToken = default);
}
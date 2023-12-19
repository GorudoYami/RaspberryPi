using GorudoYami.Common.Modules;
using System.Net;

namespace RaspberryPi.Common.Modules;

public interface ITcpServerModule : IModule {
	Task BroadcastAsync(string data, bool encrypt = true, CancellationToken cancellationToken = default);
	Task BroadcastAsync(byte[] data, bool encrypt = true, CancellationToken cancellationToken = default);
	Task SendAsync(IPAddress address, byte[] data, bool encrypt = true, CancellationToken cancellationToken = default);
	void Start();
	Task StopAsync();
}
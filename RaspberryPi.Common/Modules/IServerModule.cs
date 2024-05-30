using GorudoYami.Common.Modules;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Modules {
	public interface IServerModule : IModule {
		Task BroadcastAsync(string data, CancellationToken cancellationToken = default);
		Task BroadcastAsync(byte[] data, CancellationToken cancellationToken = default);
		Task BroadcastVideoAsync(byte[] data, CancellationToken cancellationToken = default);
		Task SendAsync(IPAddress address, byte[] data, CancellationToken cancellationToken = default);
		void Start();
		Task StopAsync();
	}
}
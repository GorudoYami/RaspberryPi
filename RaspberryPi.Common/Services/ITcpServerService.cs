using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Services {
	public interface ITcpServerService : IService {
		Task BroadcastAsync(string data, CancellationToken cancellationToken = default);
		Task BroadcastAsync(byte[] data, CancellationToken cancellationToken = default);
		Task SendAsync(IPAddress address, byte[] data, CancellationToken cancellationToken = default);
		void Start();
		Task StopAsync();
	}
}
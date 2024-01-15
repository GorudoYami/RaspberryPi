using GorudoYami.Common.Modules;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Modules.Providers {
	public interface INetworkingProvider : IModule {
		bool Connected { get; }

		Task ConnectAsync(CancellationToken cancellationToken = default);
		Task DisconnectAsync(CancellationToken cancellationToken = default);
		Task<byte[]> ReadAsync(CancellationToken cancellationToken = default);
		Task<string> ReadLineAsync(CancellationToken cancellationToken = default);
		Task SendAsync(byte[] data, CancellationToken cancellationToken = default);
		Task SendAsync(string data, CancellationToken cancellationToken = default);
	}
}

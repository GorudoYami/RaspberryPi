using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Services.Providers {
	public interface INetworkingProvider : IService {
		bool Connected { get; }

		Task ConnectAsync(CancellationToken cancellationToken = default);
		Task DisconnectAsync();
		Task<byte[]> ReadAsync(CancellationToken cancellationToken = default);
		Task<string> ReadLineAsync(CancellationToken cancellationToken = default);
		Task SendAsync(byte[] data, CancellationToken cancellationToken = default);
		Task SendAsync(string data, CancellationToken cancellationToken = default);
	}
}

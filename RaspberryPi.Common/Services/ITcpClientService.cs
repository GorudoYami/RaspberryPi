using RaspberryPi.Common.Services.Providers;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Services;
public interface ITcpClientService : INetworkingProvider {
	Task ConnectVideoAsync(CancellationToken cancellationToken = default);
}
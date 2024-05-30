using RaspberryPi.Common.Modules.Providers;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Modules {
	public interface IClientModule : INetworkingProvider {
		Task ConnectVideoAsync(CancellationToken cancellationToken = default);
	}
}
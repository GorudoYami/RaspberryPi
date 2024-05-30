using System.Threading.Tasks;
using System.Threading;
using GorudoYami.Common.Modules;

namespace RaspberryPi.Common.Modules {
	public interface INetworkModule : IModule {
		bool Connected { get; }

		Task ConnectAsync(CancellationToken cancellationToken = default);
		Task DisconnectAsync(CancellationToken cancellationToken = default);
	}
}

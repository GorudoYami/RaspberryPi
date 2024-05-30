using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Modules {
	public interface IModule {
		bool Enabled { get; }
		bool IsInitialized { get; }

		Task InitializeAsync(CancellationToken cancellationToken = default);
	}
}

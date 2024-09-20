using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Services {
	public interface IInitializableService : IService {
		bool IsInitialized { get; }
		Task InitializeAsync(CancellationToken cancellationToken = default);
	}
}

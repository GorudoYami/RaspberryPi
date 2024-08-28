using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Services;
public interface IConnectableService {
	bool Connected { get; }
	Task ConnectAsync(CancellationToken cancellationToken = default);
	Task DisconnectAsync(CancellationToken cancellationToken = default);
}

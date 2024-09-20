using RaspberryPi.Common.Services.Providers;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Services {
	public interface IModemService : INetworkingProvider {
		bool SendCommand(string command, bool throwOnFail = false, bool clearBuffer = true);
		Task<bool> WaitUntilExpectedResponse(string command, int timeoutSeconds, CancellationToken cancellationToken = default);
	}
}

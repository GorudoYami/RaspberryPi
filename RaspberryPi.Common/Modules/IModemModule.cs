using RaspberryPi.Common.Modules.Providers;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Modules {
	public interface IModemModule : INetworkingProvider {
		bool SendCommand(string command, bool throwOnFail = false, bool clearBuffer = true);
		Task<bool> WaitUntilExpectedResponse(string command, int timeoutSeconds, CancellationToken cancellationToken = default);
	}
}

using RaspberryPi.Common.Modules.Providers;

namespace RaspberryPi.Common.Modules {
	public interface IModemModule : INetworkingProvider {
		bool SendCommand(string command, bool throwOnFail = false, bool clearBuffer = true);
		bool WaitUntilExpectedResponse(string command, int timeoutSeconds);
	}
}

using RaspberryPi.Common.Modules.Providers;

namespace RaspberryPi.Common.Providers {
	public interface INetworkingResolver {
		INetworkingProvider GetNetworking();
	}
}

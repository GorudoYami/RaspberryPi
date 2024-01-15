using RaspberryPi.Common.Modules.Providers;

namespace RaspberryPi.Common.Resolvers {
	public interface INetworkingResolver {
		INetworkingProvider GetNetworking();
	}
}

using RaspberryPi.Common.Services.Providers;

namespace RaspberryPi.Common.Resolvers {
	public interface INetworkingResolver {
		INetworkingProvider GetNetworking();
	}
}

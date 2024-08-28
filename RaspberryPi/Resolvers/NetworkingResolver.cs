using RaspberryPi.Common.Resolvers;
using RaspberryPi.Common.Services;
using RaspberryPi.Common.Services.Providers;

namespace RaspberryPi.Resolvers;
public class NetworkingResolver(
	IModemService modemModule,
	ITcpClientService clientModule) : INetworkingResolver {
	public INetworkingProvider GetNetworking() {
		INetworkingProvider defaultNetworking = clientModule;
		INetworkingProvider modemNetworking = modemModule;

		if (defaultNetworking.Connected) {
			return defaultNetworking;
		}

		if (modemNetworking.Connected) {
			return modemNetworking;
		}

		return null;
	}
}

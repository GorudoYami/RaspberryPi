using RaspberryPi.Common.Resolvers;
using RaspberryPi.Common.Services;
using RaspberryPi.Common.Services.Providers;

namespace RaspberryPi.Resolvers;
public class NetworkingResolver : INetworkingResolver {
	private readonly IModemService _modemModule;
	private readonly ITcpClientService _clientModule;

	public NetworkingResolver(IModemService modemModule, ITcpClientService clientModule) {
		_modemModule = modemModule;
		_clientModule = clientModule;
	}

	public INetworkingProvider GetNetworking() {
		INetworkingProvider defaultNetworking = _clientModule;
		INetworkingProvider modemNetworking = _modemModule;

		if (defaultNetworking.Connected) {
			return defaultNetworking;
		}

		if (modemNetworking.Connected) {
			return modemNetworking;
		}

		return null;
	}
}

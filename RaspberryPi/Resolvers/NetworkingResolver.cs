using RaspberryPi.Common.Resolvers;
using RaspberryPi.Common.Services;
using RaspberryPi.Common.Services.Providers;

namespace RaspberryPi.Resolvers {
	public class NetworkingResolver : INetworkingResolver {
		private readonly IModemService _modemService;
		private readonly ITcpClientService _tcpClientService;

		public NetworkingResolver(IModemService modemModule, ITcpClientService clientModule) {
			_modemService = modemModule;
			_tcpClientService = clientModule;
		}

		public INetworkingProvider GetNetworking() {
			INetworkingProvider defaultNetworking = _modemService;
			INetworkingProvider modemNetworking = _tcpClientService;

			if (defaultNetworking.Connected) {
				return defaultNetworking;
			}

			if (modemNetworking.Connected) {
				return modemNetworking;
			}

			return null;
		}
	}
}

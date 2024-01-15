using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Modules.Providers;
using RaspberryPi.Common.Providers;

namespace RaspberryPi.Resolvers {
	public class NetworkingResolver : INetworkingResolver {
		private readonly IModemModule _modemModule;
		private readonly IClientModule _clientModule;

		public NetworkingResolver(IModemModule modemModule, IClientModule clientModule) {
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
}

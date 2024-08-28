using RaspberryPi.Common.Resolvers;
using RaspberryPi.Common.Services;
using RaspberryPi.Common.Services.Providers;

namespace RaspberryPi.Resolvers {
	public class MqttResolver : IMqttResolver {
		private readonly IModemMqttService _modemMqttModule;
		private readonly IMqttClientService _mqttModule;

		public MqttResolver(IModemMqttService modemMqttModule, IMqttClientService mqttModule) {
			_modemMqttModule = modemMqttModule;
			_mqttModule = mqttModule;
		}

		public IMqttProvider GetMqtt() {
			if (_mqttModule.Connected) {
				return _mqttModule;
			}

			//if (_modemMqttModule.Connected) {
			//	return _modemMqttModule;
			//}

			return null;
		}
	}
}

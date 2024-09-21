using RaspberryPi.Common.Resolvers;
using RaspberryPi.Common.Services;
using RaspberryPi.Common.Services.Providers;

namespace RaspberryPi.Resolvers {
	public class MqttResolver : IMqttResolver {
		private readonly IModemMqttService _modemMqttModule;
		private readonly IMqttClientService _mqttClientService;

		public MqttResolver(IModemMqttService modemMqttModule, IMqttClientService mqttClientService) {
			_modemMqttModule = modemMqttModule;
			_mqttClientService = mqttClientService;
		}
		public IMqttProvider GetMqtt() {
			if (_mqttClientService.Connected) {
				return _mqttClientService;
			}

			if (_modemMqttModule.Enabled) {
				return _modemMqttModule;
			}

			return null;
		}
	}
}

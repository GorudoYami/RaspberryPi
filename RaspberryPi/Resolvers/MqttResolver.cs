using RaspberryPi.Common.Resolvers;
using RaspberryPi.Common.Services;
using RaspberryPi.Common.Services.Providers;

namespace RaspberryPi.Resolvers {
	public class MqttResolver(
		IModemMqttService modemMqttModule,
		IMqttClientService mqttModule) : IMqttResolver {
		public IMqttProvider GetMqtt() {
			if (mqttModule.Connected) {
				return mqttModule;
			}

			if (modemMqttModule.Enabled) {
				return modemMqttModule;
			}

			return null;
		}
	}
}

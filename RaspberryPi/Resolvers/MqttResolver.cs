using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Modules.Providers;
using RaspberryPi.Common.Resolvers;

namespace RaspberryPi.Resolvers {
	public class MqttResolver : IMqttResolver {
		private readonly IModemMqttModule _modemMqttModule;
		private readonly IMqttModule _mqttModule;

		public MqttResolver(IModemMqttModule modemMqttModule, IMqttModule mqttModule) {
			_modemMqttModule = modemMqttModule;
			_mqttModule = mqttModule;
		}

		public IMqttProvider GetMqtt() {
			if (_mqttModule.Connected) {
				return _mqttModule;
			}

			if (_modemMqttModule.Connected) {
				return _modemMqttModule;
			}

			return null;
		}
	}
}

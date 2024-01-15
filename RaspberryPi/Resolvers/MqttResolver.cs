using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Modules.Providers;
using RaspberryPi.Common.Providers;

namespace RaspberryPi.Providers;

public class MqttResolver : IMqttResolver {
	private readonly IModemModule _modemModule;
	private readonly IMqttModule _mqttModule;

	public MqttResolver(IModemModule modemModule, IMqttModule mqttModule) {
		_modemModule = modemModule;
		_mqttModule = mqttModule;
	}

	public IMqttProvider? GetMqtt() {
		IMqttModule defaultMqtt = _mqttModule;
		IMqttModule modemMqtt = _modemModule;

		if (defaultMqtt.Connected) {
			return defaultMqtt;
		}

		if (modemMqtt.Connected) {
			return modemMqtt;
		}

		return null;
	}
}

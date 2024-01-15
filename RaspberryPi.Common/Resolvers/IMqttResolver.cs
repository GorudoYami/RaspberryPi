using RaspberryPi.Common.Modules.Providers;

namespace RaspberryPi.Common.Providers {
	public interface IMqttResolver {
		IMqttProvider GetMqtt();
	}
}

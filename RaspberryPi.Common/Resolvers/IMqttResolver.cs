using RaspberryPi.Common.Modules.Providers;

namespace RaspberryPi.Common.Resolvers {
	public interface IMqttResolver {
		IMqttProvider GetMqtt();
	}
}

using RaspberryPi.Common.Services.Providers;

namespace RaspberryPi.Common.Resolvers;
public interface IMqttResolver {
	IMqttProvider GetMqtt();
}

using GorudoYami.Common.Modules;

namespace RaspberryPi.Common.Modules;

public interface ISensorsModule : IModule {
	void ResetSensor(string sensorName);
	void Start();
	Task StopAsync();
}

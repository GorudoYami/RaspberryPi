using RaspberryPi.Common.Events;

namespace RaspberryPi.Common.Services;

public interface ISensorService : IService {
	event EventHandler<SensorTriggeredEventArgs>? SensorTriggered;

	void ResetSensor(string sensorName);
	void Start();
	Task StopAsync();
}

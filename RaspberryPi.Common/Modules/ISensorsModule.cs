using GorudoYami.Common.Modules;
using RaspberryPi.Common.Events;
using System;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Modules {
	public interface ISensorsModule : IModule {
		event EventHandler<SensorTriggeredEventArgs> SensorTriggered;

		void ResetSensor(string sensorName);
		void Start();
		Task StopAsync();
	}
}

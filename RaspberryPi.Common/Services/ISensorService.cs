﻿using RaspberryPi.Common.Events;
using System;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Services {
	public interface ISensorService : IInitializableService {
		event EventHandler<SensorTriggeredEventArgs> SensorTriggered;

		void ResetSensor(string sensorName);
		void Start();
		Task StopAsync();
	}
}

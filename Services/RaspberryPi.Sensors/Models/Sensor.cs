using RaspberryPi.Sensors.Enums;
using System.Collections.Generic;

namespace RaspberryPi.Sensors.Models {
	public class Sensor {
		public string Name { get; set; }
		public Dictionary<SensorPinType, int> Pins { get; set; }

		private bool _triggered;

		public bool IsTriggered() {
			return _triggered;
		}

		public void SetTriggered() {
			_triggered = true;
		}

		public void Reset() {
			_triggered = false;
		}
	}
}

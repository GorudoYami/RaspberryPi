using RaspberryPi.Sensors.Enums;

namespace RaspberryPi.Sensors.Models {
	public class Sensor {
		public required string Name { get; init; }
		public required Dictionary<SensorPinType, int> Pins { get; init; }

		private bool _triggered;

		public Sensor() {
			_triggered = false;
		}

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

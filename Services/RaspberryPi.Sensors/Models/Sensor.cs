using RaspberryPi.Sensors.Enums;

namespace RaspberryPi.Sensors.Models {
	public class Sensor(string name, Dictionary<SensorPinType, int> pins) {
		public string Name { get; } = name;
		public Dictionary<SensorPinType, int> Pins { get; } = pins;

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

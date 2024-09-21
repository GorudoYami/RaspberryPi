using RaspberryPi.Driving.Enums;

namespace RaspberryPi.Driving.Models {
	public class DrivingPin {
		public int Number { get; }
		public DrivingPinType Type { get; }

		public DrivingPin(int number, DrivingPinType type) {
			Number = number;
			Type = type;
		}
	}
}
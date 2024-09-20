using RaspberryPi.Driving.Enums;

namespace RaspberryPi.Driving.Models {
	public class DrivingPin(int number, DrivingPinType type) {
		public int Number { get; } = number;
		public DrivingPinType Type { get; } = type;
	}
}
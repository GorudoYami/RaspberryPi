using RaspberryPi.Driving.Enums;

namespace RaspberryPi.Driving.Models {
	public class DrivingPwmPin(int number, Direction direction, int chip, int frequency)
		: DrivingPin(number, direction) {
		public int Chip { get; init; } = chip;
		public int Frequency { get; init; } = frequency;
	}
}

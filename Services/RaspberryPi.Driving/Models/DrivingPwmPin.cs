using RaspberryPi.Driving.Enums;

namespace RaspberryPi.Driving.Models;
public class DrivingPwmPin : DrivingPin {
	public int Chip { get; }
	public int Frequency { get; }

	public DrivingPwmPin(int number, Direction direction, int chip, int frequency)
		: base(number, direction) {
		Chip = chip;
		Frequency = frequency;
	}
}

namespace RaspberryPi.Driving.Models;

public class DrivingPwmPin(int number, Direction direction, int pwmChip, int frequency)
	: DrivingPin(number, direction) {
	public required int Chip { get; init; } = pwmChip;
	public required int Frequency { get; init; } = frequency;
}

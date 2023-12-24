using RaspberryPi.Modules.Enums;
using System.Device.Gpio;

namespace RaspberryPi.Modules.Models;

public class DrivingPwmPin(int number, Direction direction, int pwmChip, int frequency)
	: DrivingPin(number, direction) {
	public required int Chip { get; init; } = pwmChip;
	public required int Frequency { get; init; } = frequency;
}

using RaspberryPi.Modules.Enums;
using System.Device.Gpio;

namespace RaspberryPi.Modules.Models;

public class PwmPin(int number, Direction direction, int pwmChip, int frequency) : IDrivingPin {
	public required int Number { get; init; } = number;
	public required Direction Direction { get; init; } = direction;
	public required int PwmChip { get; init; } = pwmChip;
	public required int Frequency { get; init; } = frequency;
}

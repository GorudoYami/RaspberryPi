using RaspberryPi.Modules.Enums;
using System.Device.Gpio;

namespace RaspberryPi.Modules.Models;

public class PwmPin(int number, PinMode mode, Direction direction, int pwmChip, int frequency) : IPin {
	public required int Number { get; init; } = number;
	public required PinMode Mode { get; init; } = mode;
	public required Direction Direction { get; init; } = direction;
	public required int PwmChip { get; init; } = pwmChip;
	public required int Frequency { get; init; } = frequency;
}

using RaspberryPi.Modules.Enums;
using System.Device.Gpio;

namespace RaspberryPi.Modules.Models;

public class Pin(int number, PinMode mode, Direction direction) : IPin {
	public required int Number { get; init; } = number;
	public required PinMode Mode { get; init; } = mode;
	public required Direction Direction { get; init; } = direction;
}

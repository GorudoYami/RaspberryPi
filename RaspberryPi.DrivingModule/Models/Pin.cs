using RaspberryPi.Modules.Enums;
using System.Device.Gpio;

namespace RaspberryPi.Modules.Models;

public class Pin(int number, Direction direction) : IDrivingPin {
	public required int Number { get; init; } = number;
	public required Direction Direction { get; init; } = direction;
}

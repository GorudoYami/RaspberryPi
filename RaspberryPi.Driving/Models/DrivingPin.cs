using RaspberryPi.Modules.Enums;

namespace RaspberryPi.Modules.Models;

public class DrivingPin(int number, Direction direction) {
	public int Number { get; init; } = number;
	public Direction Direction { get; init; } = direction;
}
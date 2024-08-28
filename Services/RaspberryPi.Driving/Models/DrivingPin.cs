using RaspberryPi.Driving.Enums;

namespace RaspberryPi.Driving.Models;
public class DrivingPin {
	public int Number { get; }
	public Direction Direction { get; }

	public DrivingPin(int number, Direction direction) {
		Number = number;
		Direction = direction;
	}
}
namespace RaspberryPi.Driving.Models;

public class DrivingModuleOptions(ICollection<DrivingPin> pins) {
	public required ICollection<DrivingPin> Pins { get; init; } = pins;
}

namespace RaspberryPi.Modules.Models;

public class DrivingModuleOptions {
	public required ICollection<IDrivingPin> Pins { get; init; }
}

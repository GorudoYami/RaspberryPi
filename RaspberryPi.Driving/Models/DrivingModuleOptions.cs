
namespace RaspberryPi.Driving.Models;

public class DrivingModuleOptions {
	public required ICollection<DrivingPin> Pins { get; init; }

	public static bool Validate(DrivingModuleOptions options) {
		throw new NotImplementedException();
	}
}

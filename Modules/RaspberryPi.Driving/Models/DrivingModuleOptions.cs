
using RaspberryPi.Driving.Enums;

namespace RaspberryPi.Driving.Models;

public class DrivingModuleOptions {
	public required ICollection<DrivingPin> Pins { get; init; }

	public static bool Validate(DrivingModuleOptions options) {
		return options.Pins.Any(x => x.Direction == Direction.Left) &&
			options.Pins.Any(x => x.Direction == Direction.Right) &&
			options.Pins.Any(x => x.Direction == (Direction.Forward | Direction.Back));
	}
}

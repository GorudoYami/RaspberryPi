using RaspberryPi.Common.Options;
using RaspberryPi.Driving.Enums;
using RaspberryPi.Driving.Models;

namespace RaspberryPi.Driving.Options;
public class DrivingOptions : IServiceOptions {
	public bool Enabled { get; set; }
	public required ICollection<DrivingPin> Pins { get; init; }

	public static bool Validate(DrivingOptions options) {
		return options.Pins.Any(x => x.Direction == Direction.Left) &&
			options.Pins.Any(x => x.Direction == Direction.Right) &&
			options.Pins.Any(x => x.Direction == Direction.Forward) &&
			options.Pins.Any(x => x.Direction == Direction.Back);
	}
}

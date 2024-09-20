using RaspberryPi.Common.Options;
using RaspberryPi.Driving.Enums;
using RaspberryPi.Driving.Models;

namespace RaspberryPi.Driving.Options {
	public class DrivingOptions : IServiceOptions {
		public bool Enabled { get; set; }
		public required ICollection<DrivingPin> Pins { get; init; }
		public required ICollection<DrivingPwmPin> PwmPins { get; init; }

		public static bool Validate(DrivingOptions options) {
			return options.PwmPins.Any(x => x.Type == DrivingPinType.Steering) &&
				options.PwmPins.Any(x => x.Type == DrivingPinType.Driving) &&
				options.Pins.Any(x => x.Type == DrivingPinType.Forward) &&
				options.Pins.Any(x => x.Type == DrivingPinType.Backward);
		}
	}
}

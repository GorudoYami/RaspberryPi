using RaspberryPi.Common.Options;
using RaspberryPi.Driving.Enums;
using RaspberryPi.Driving.Models;
using System.Collections.Generic;
using System.Linq;

namespace RaspberryPi.Driving.Options {
	public class DrivingOptions : IServiceOptions {
		public bool Enabled { get; set; }
		public ICollection<DrivingPin> Pins { get; set; }
		public ICollection<DrivingPwmPin> PwmPins { get; set; }

		public static bool Validate(DrivingOptions options) {
			return options.PwmPins.Any(x => x.Type == DrivingPinType.Steering) &&
				options.PwmPins.Any(x => x.Type == DrivingPinType.Driving) &&
				options.Pins.Any(x => x.Type == DrivingPinType.Forward) &&
				options.Pins.Any(x => x.Type == DrivingPinType.Backward);
		}
	}
}

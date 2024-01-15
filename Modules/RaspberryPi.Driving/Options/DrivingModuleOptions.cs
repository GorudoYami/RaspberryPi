using RaspberryPi.Driving.Enums;
using RaspberryPi.Driving.Models;
using System.Collections.Generic;
using System.Linq;

namespace RaspberryPi.Driving.Options {
	public class DrivingModuleOptions {
		public ICollection<DrivingPin> Pins { get; set; }

		public static bool Validate(DrivingModuleOptions options) {
			return options.Pins.Any(x => x.Direction == Direction.Left) &&
				options.Pins.Any(x => x.Direction == Direction.Right) &&
				options.Pins.Any(x => x.Direction == (Direction.Forward | Direction.Back));
		}
	}
}

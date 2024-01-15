using RaspberryPi.Sensors.Models;
using System.Collections.Generic;
using System.Linq;

namespace RaspberryPi.Sensors.Options {
	public class SensorsModuleOptions {
		public int PoolingPeriod { get; set; }
		public int ReportDistance { get; set; }
		public ICollection<Sensor> Sensors { get; set; }

		public static bool Validate(SensorsModuleOptions options) {
			if (options.Sensors.GroupBy(x => x.Name).Any(x => x.Count() > 1)) {
				return false;
			}

			foreach (Sensor sensor in options.Sensors) {
				if (sensor.Pins.Keys.Count < 2) {
					return false;
				}
			}

			return true;
		}
	}
}

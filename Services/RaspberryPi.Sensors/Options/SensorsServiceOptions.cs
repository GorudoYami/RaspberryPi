using RaspberryPi.Sensors.Models;

namespace RaspberryPi.Sensors.Options;

public class SensorsServiceOptions {
	public required int PoolingPeriod { get; init; }
	public required int ReportDistance { get; init; }
	public required ICollection<Sensor> Sensors { get; init; }

	public static bool Validate(SensorsServiceOptions options) {
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

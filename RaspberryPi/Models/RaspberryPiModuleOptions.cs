namespace RaspberryPi.Models {
	public class RaspberryPiModuleOptions {
		public required int ReconnectPeriodSeconds { get; init; }
		public required int PingTimeoutSeconds { get; init; }
		public required bool DefaultSafety { get; init; }

		public static bool Validate(RaspberryPiModuleOptions options) {
			return true;
		}
	}
}

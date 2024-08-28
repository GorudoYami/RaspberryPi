namespace RaspberryPi.Options {
	public class RaspberryPiModuleOptions {
		public int ReconnectPeriodSeconds { get; set; }
		public int PingTimeoutSeconds { get; set; }
		public bool DefaultSafety { get; set; }

		public static bool Validate(RaspberryPiModuleOptions options) {
			return true;
		}
	}
}

namespace RaspberryPi.Mqtt.Models {
	public class MqttModuleOptions {
		public required string ServerHost { get; init; }
		public required int ServerPort { get; init; }

		public static bool Validate(MqttModuleOptions options) {
			return true;
		}
	}
}

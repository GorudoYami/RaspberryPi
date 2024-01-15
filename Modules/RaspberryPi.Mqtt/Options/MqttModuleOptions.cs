namespace RaspberryPi.Mqtt.Options {
	public class MqttModuleOptions {
		public string ServerHost { get; set; }
		public int ServerPort { get; set; }

		public static bool Validate(MqttModuleOptions options) {
			return true;
		}
	}
}

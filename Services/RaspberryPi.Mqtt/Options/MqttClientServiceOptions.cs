namespace RaspberryPi.Mqtt.Options {
	public class MqttClientServiceOptions {
		public bool Enabled { get; set; }
		public string ServerHost { get; set; }
		public int ServerPort { get; set; }

		public static bool Validate(MqttClientServiceOptions options) {
			return true;
		}
	}
}

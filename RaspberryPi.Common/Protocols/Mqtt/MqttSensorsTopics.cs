namespace RaspberryPi.Common.Protocols.Mqtt {
	public class MqttSensorsTopics {
		public string Initialized { get; }

		public MqttSensorsTopics() {
			Initialized = "sensors/initialized";
		}
	}
}

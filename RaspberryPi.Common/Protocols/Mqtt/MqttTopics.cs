namespace RaspberryPi.Common.Protocols.Mqtt {
	public class MqttTopics {
		public MqttDrivingTopics Driving { get; }
		public MqttSensorsTopics Sensors { get; }
		public MqttCameraTopics Camera { get; }

		public MqttTopics() {
			Driving = new MqttDrivingTopics();
			Sensors = new MqttSensorsTopics();
			Camera = new MqttCameraTopics();
		}
	}
}

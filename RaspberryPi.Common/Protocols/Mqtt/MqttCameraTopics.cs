namespace RaspberryPi.Common.Protocols.Mqtt {
	public class MqttCameraTopics {
		public string Initialized { get; }
		public string Capturing { get; }

		public MqttCameraTopics() {
			Initialized = "camera/initialized";
			Capturing = "camera/capturing";
		}
	}
}

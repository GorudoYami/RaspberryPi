namespace RaspberryPi.Common.Models.Mqtt {
	public class MqttTopic {
		public string Name { get; }
		public string Value { get; private set; }

		public MqttTopic(string name, string value) {
			Name = name;
			Value = value;
		}

		public void UpdateValue(string value) {
			lock (this) {
				Value = value;
			}
		}
	}
}

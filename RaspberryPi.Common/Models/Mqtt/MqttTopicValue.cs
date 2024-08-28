using System;

namespace RaspberryPi.Common.Models.Mqtt;
public class MqttTopicValue {
	public string Value { get; }
	public DateTime Received { get; }

	public MqttTopicValue(string value, DateTime received) {
		Value = value;
		Received = received;
	}
}

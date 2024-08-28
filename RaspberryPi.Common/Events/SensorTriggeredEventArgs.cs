using System;

namespace RaspberryPi.Common.Events;
public class SensorTriggeredEventArgs : EventArgs {
	public string Name { get; }
	public int Distance { get; }

	public SensorTriggeredEventArgs(string name, int distance) {
		Name = name;
		Distance = distance;
	}
}


namespace RaspberryPi.Sensors.Models;

public class SensorTriggeredEventArgs(string name, int distance) : EventArgs {
	public string Name { get; } = name;
	public int Distance { get; } = distance;
}

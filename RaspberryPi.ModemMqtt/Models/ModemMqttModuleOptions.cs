namespace RaspberryPi.ModemMqtt.Models;

public class ModemMqttModuleOptions {
	public required string ServerHost { get; init; }
	public required int ServerPort { get; init; }
	public required int QosLevel { get; init; }
}

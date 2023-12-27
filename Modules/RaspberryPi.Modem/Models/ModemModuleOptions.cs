namespace RaspberryPi.Modem.Models;

public class ModemModuleOptions {
	public required string SerialPort { get; init; }
	public required int DefaultTimeoutSeconds { get; init; }
	public required int DefaultBaudRate { get; init; }
	public required int TargetBaudRate { get; init; }
	public required string ServerHost { get; init; }
	public required int ServerPort { get; init; }

	public static bool Validate(ModemModuleOptions options) {
		throw new NotImplementedException();
	}
}

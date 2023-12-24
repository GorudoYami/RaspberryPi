
namespace RaspberryPi.Modem.Models;

public class ModemModuleOptions {
	public required string SerialPort { get; init; }
	public required int DefaultBaudRate { get; init; }
	public required int TargetBaudRate { get; init; }

	public static bool Validate(ModemModuleOptions options) {
		throw new NotImplementedException();
	}
}

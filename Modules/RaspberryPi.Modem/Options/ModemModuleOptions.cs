using RaspberryPi.Modem.Models;

namespace RaspberryPi.Modem.Options;

public class ModemModuleOptions {
	public required string SerialPort { get; init; }
	public required int DefaultBaudRate { get; init; }
	public required int TargetBaudRate { get; init; }
	public required int ServerPort { get; init; }
	public required int TimeoutSeconds { get; init; }
	public required ICollection<ExpectedResponse> ExpectedResponses { get; init; }

	public static bool Validate(ModemModuleOptions options) {
		return true; //zzz
	}
}


namespace RaspberryPi.Client.Models;

public class ClientModuleOptions {
	public required string ServerHost { get; init; }
	public required int ServerPort { get; init; }
	public required int TimeoutSeconds { get; init; }

	public static bool Validate(ClientModuleOptions options) {
		throw new NotImplementedException();
	}
}

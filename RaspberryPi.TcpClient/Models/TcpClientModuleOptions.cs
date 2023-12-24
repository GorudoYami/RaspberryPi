namespace RaspberryPi.TcpClient.Models;

public class TcpClientModuleOptions {
	public required string ServerHost { get; init; }
	public required int ServerPort { get; init; }
	public required int TimeoutSeconds { get; init; }
}

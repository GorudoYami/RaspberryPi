namespace RaspberryPi.TcpServer.Models;

public class TcpServerModuleOptions {
	public required string Host { get; init; }
	public required int Port { get; init; }
}

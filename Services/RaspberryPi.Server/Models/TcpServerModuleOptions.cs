using RaspberryPi.Common.Options;

namespace RaspberryPi.TcpServer.Models;
public class TcpServerModuleOptions : IServiceOptions {
	public bool Enabled { get; init; }
	public required string Host { get; init; }
	public int MainPort { get; init; }
	public int VideoPort { get; init; }
}

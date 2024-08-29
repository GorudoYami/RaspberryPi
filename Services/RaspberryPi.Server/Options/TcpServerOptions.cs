using RaspberryPi.Common.Options;

namespace RaspberryPi.TcpServer.Options;

public class TcpServerOptions : IServiceOptions {
	public bool Enabled { get; set; }
	public required string Host { get; set; }
	public int MainPort { get; set; }
	public int VideoPort { get; set; }
}

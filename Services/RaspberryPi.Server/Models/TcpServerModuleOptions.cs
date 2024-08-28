using RaspberryPi.Common.Options;

namespace RaspberryPi.TcpServer.Models;
public class TcpServerModuleOptions : IServiceOptions {
	public bool Enabled { get; set; }
	public string Host { get; set; }
	public int MainPort { get; set; }
	public int VideoPort { get; set; }
}

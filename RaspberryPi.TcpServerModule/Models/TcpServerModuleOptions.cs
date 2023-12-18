namespace RaspberryPi.Modules.Models;

public class TcpServerModuleOptions {
	public string Host { get; }
	public int Port { get; }

	public TcpServerModuleOptions(string host, int port) {
		Host = host;
		Port = port;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaspberryPi.Modules.Models;

public class TcpClientOptions {
	public string ServerHost { get; }
	public int ServerPort { get; }
	public int TimeoutSeconds { get; }

	public TcpClientOptions(string serverHost, int serverPort, int timeoutSeconds) {
		ServerHost = serverHost;
		ServerPort = serverPort;
		TimeoutSeconds = timeoutSeconds;
	}
}

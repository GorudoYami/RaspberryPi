using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaspberryPi.Modules.Models;

public class TcpClientModuleOptions {
	public required string ServerHost { get; init; }
	public required int ServerPort { get; init; }
	public required int TimeoutSeconds { get; init; }
}

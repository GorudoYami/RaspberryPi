using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace RaspberryPi.TcpServerModule.Models;

public class ClientInfo {
	public Aes Aes { get; }
	public Stream Stream { get; }

	public ClientInfo(Aes aes, Stream stream) {
		Aes = aes;
		Stream = stream;
	}
}

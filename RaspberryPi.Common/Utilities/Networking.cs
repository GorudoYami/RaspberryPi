using System.Net;
using System.Net.Sockets;

namespace RaspberryPi.Common;

public static class Networking {
	public static IPAddress GetAddressFromHostname(string hostname, AddressFamily addressFamily = AddressFamily.InterNetwork) {
		var host = Dns.GetHostEntry(hostname, addressFamily);
		return host.AddressList[0];
	}
}

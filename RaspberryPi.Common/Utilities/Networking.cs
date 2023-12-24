using System.Net;
using System.Net.Sockets;

namespace RaspberryPi.Common.Utilities;

public static class Networking {
	public static IPAddress GetAddressFromHostname(string hostname, AddressFamily addressFamily = AddressFamily.InterNetwork) {
		IPHostEntry host = Dns.GetHostEntry(hostname, addressFamily);
		return host.AddressList[0];
	}
}

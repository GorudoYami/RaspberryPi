using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace RaspberryPi.Common.Utilities;

public static class Networking {
	public static IPAddress GetAddressFromHostname(string hostname) {
		if (IPAddress.TryParse(hostname, out IPAddress? ipAddress) && ipAddress != null) {
			return ipAddress;
		}

		return Dns.GetHostEntry(hostname)
			.AddressList
			.Where(x => x.AddressFamily == AddressFamily.InterNetwork)
			.First();
	}
}

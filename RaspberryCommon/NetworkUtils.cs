using System.Net;

namespace RaspberryPi.Common;

public static class NetworkUtils {
	public static IPAddress GetAddressFromHostname(string hostname) {
		var host = Dns.GetHostEntry(hostname);
		return host.AddressList[0];
	}
}

using System.Net;

namespace RaspberryPi.Common.Utilities {
	public static class Networking {
		public static IPAddress GetAddressFromHostname(string hostname) {
			if (IPAddress.TryParse(hostname, out IPAddress ipAddress)) {
				return ipAddress;
			}

			IPHostEntry host = Dns.GetHostEntry(hostname);
			return host.AddressList[0];
		}
	}
}

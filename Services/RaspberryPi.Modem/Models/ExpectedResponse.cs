using System.Collections.Generic;

namespace RaspberryPi.Modem.Models {
	public class ExpectedResponse {
		public string Command { get; set; }
		public IReadOnlyCollection<string> ResponseLines { get; set; }
		public bool MatchAny { get; set; }
	}
}

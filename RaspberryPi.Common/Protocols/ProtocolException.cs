using System;

namespace RaspberryPi.Common.Protocols {
	public class ProtocolException : Exception {
		public ProtocolException(string message) : base(message) { }
	}
}

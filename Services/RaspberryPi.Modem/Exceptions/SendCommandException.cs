using System;

namespace RaspberryPi.Modem.Exceptions {
	public class SendCommandException : Exception {
		public SendCommandException(string message) : base(message) { }
		public SendCommandException(string message, Exception innerException) : base(message, innerException) { }
	}
}

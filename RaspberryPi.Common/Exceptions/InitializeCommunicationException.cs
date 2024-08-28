using System;

namespace RaspberryPi.Common.Exceptions;
public class InitializeCommunicationException : Exception {
	public InitializeCommunicationException(string message) : base(message) { }
	public InitializeCommunicationException(string message, Exception innerException) : base(message, innerException) { }
}

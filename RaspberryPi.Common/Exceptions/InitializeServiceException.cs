using System;

namespace RaspberryPi.Common.Exceptions;
public class InitializeServiceException : Exception {
	public InitializeServiceException(string message) : base(message) { }
	public InitializeServiceException(string message, Exception innerException) : base(message, innerException) { }
	public InitializeServiceException() : base() { }
}

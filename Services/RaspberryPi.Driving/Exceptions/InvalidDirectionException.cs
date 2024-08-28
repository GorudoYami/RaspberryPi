using System;

namespace RaspberryPi.Driving.Exceptions;
public class InvalidDirectionException : Exception {
	public InvalidDirectionException(string message) : base(message) { }
}

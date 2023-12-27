using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RasperryPi.Common.Tests;

public static class MockedLoggerProvider {
	public static Mock<ILogger<T>> GetMockedLogger<T>() {
		var mockedLogger = new Mock<ILogger<T>>();
		mockedLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
		mockedLogger.Setup(x => x.Log(
			It.IsAny<LogLevel>(),
			It.IsAny<EventId>(),
			It.IsAny<object>(),
			It.IsAny<Exception?>(),
			It.IsAny<Func<object, Exception?, string>>()))
			.Callback(LogCallback);

		return mockedLogger;
	}

	private static void LogCallback(
		LogLevel logLevel,
		EventId eventId,
		object state,
		Exception? exception,
		Func<object, Exception?, string> formatter) {
		Debug.WriteLine(formatter.Invoke(state, exception));
		Console.WriteLine(formatter.Invoke(state, exception));
	}
}

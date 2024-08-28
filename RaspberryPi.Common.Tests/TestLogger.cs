using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace RasperryPi.Common.Tests;

public class TestLogger<T> : ILogger<T>, IDisposable {
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
		Debug.WriteLine($"[{typeof(T).Name}] {formatter.Invoke(state, exception)}");
		Console.WriteLine($"[{typeof(T).Name}] {formatter.Invoke(state, exception)}");
	}

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
		return this;
	}

	public bool IsEnabled(LogLevel logLevel) {
		return true;
	}


	public void Dispose() {
		GC.SuppressFinalize(this);
	}
}

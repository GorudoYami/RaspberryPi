using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Common.Modules;
using RaspberryPi.Modem.Models;
using System.Diagnostics;
using System.IO.Ports;

namespace RaspberryPi.Modem.IntegrationTests;

[TestFixture]
public class ModemModuleIntegrationTests {
	private readonly ModemModuleOptions _options;

	private Mock<ILogger<IModemModule>> _mockedLogger;
	private Mock<IOptions<ModemModuleOptions>> _mockedOptions;

	private ModemModule? _modemModule;

	public ModemModuleIntegrationTests() {
		_options = new ModemModuleOptions() {
			DefaultBaudRate = 9600,
			TargetBaudRate = 4000000,
			SerialPort = "COM7",
			ServerHost = "10.0.0.1",
			ServerPort = 2137
		};
	}

	[SetUp]
	public void SetUp() {
		_mockedLogger = new Mock<ILogger<IModemModule>>();
		_mockedLogger.Setup(x => x.Log(
			It.IsAny<LogLevel>(),
			It.IsAny<EventId>(),
			It.IsAny<object>(),
			It.IsAny<Exception?>(),
			It.IsAny<Func<object, Exception?, string>>()))
			.Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>((logLevel, eventId, state, exception, formatter)
				=> Debug.WriteLine(formatter?.Invoke(state, exception)));

		_mockedOptions = new Mock<IOptions<ModemModuleOptions>>();
		_mockedOptions.Setup(x => x.Value).Returns(_options);
	}

	[TearDown]
	public void TearDown() {
		_modemModule?.Dispose();
		_modemModule = null;
	}

	private ModemModule CreateInstance() {
		return _modemModule ??= new ModemModule(_mockedOptions.Object, _mockedLogger.Object);
	}

	[Test]
	public void Initialize_DoesNotThrow() {
		CreateInstance();

		Assert.DoesNotThrowAsync(() => _modemModule!.InitializeAsync());
	}
}

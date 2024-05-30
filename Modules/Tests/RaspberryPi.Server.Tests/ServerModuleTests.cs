using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Server.Models;
using System.Diagnostics;

namespace RaspberryPi.Server.Tests;

[TestFixture]
public class ServerModuleTests {
	private ServerModule? _serverModule;
	private Mock<IOptions<ServerModuleOptions>> _mockedOptions;
	private Mock<ILogger<IServerModule>> _mockedLogger;
	private Mock<IServerProtocol> _mockedServerProtocol;

	[SetUp]
	public void SetUp() {
		_mockedOptions = new Mock<IOptions<ServerModuleOptions>>();
		_mockedLogger = new Mock<ILogger<IServerModule>>();
		_mockedLogger.Setup(x => x.Log(
			It.IsAny<LogLevel>(),
			It.IsAny<EventId>(),
			It.IsAny<object>(),
			It.IsAny<Exception?>(),
			It.IsAny<Func<object, Exception?, string>>()))
			.Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>((logLevel, eventId, state, exception, formatter)
				=> Debug.WriteLine(formatter?.Invoke(state, exception)));
		_mockedServerProtocol = new Mock<IServerProtocol>();

		_serverModule = null;
	}

	private ServerModule GetInstance() {
		return _serverModule ??= new ServerModule(
			_mockedOptions.Object,
			_mockedLogger.Object,
			_mockedServerProtocol.Object
		);
	}

	[Ignore("WIP")]
	[Test]
	public void Constructor() {
		_mockedOptions!.Setup(x => x.Value)
			.Returns(new ServerModuleOptions() {
				Host = "localhost",
				MainPort = 2137
			});

		Assert.DoesNotThrow(() => GetInstance());
	}

	[TearDown]
	public void TearDown() {
		_serverModule?.Dispose();
	}
}

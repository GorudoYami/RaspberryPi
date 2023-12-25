using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Common.Modules;
using RaspberryPi.Server.Models;

namespace RaspberryPi.Server.Tests;

[TestFixture]
public class ServerModuleTests {
	private ServerModule? _serverModule;
	private Mock<IOptions<ServerModuleOptions>>? _mockedOptions;
	private Mock<ILogger<IServerModule>>? _mockedLogger;

	[SetUp]
	public void SetUp() {
		_mockedLogger = new Mock<ILogger<IServerModule>>();
		_mockedOptions = new Mock<IOptions<ServerModuleOptions>>();
	}

	private ServerModule GetInstance() {
		return _serverModule ??= new ServerModule(_mockedOptions!.Object, _mockedLogger!.Object);
	}

	[Ignore("WIP")]
	[Test]
	public void Constructor() {
		_mockedOptions!.Setup(x => x.Value)
			.Returns(new ServerModuleOptions() {
				Host = "localhost",
				Port = 2137
			});

		Assert.DoesNotThrow(() => GetInstance());
	}

	[TearDown]
	public void TearDown() {
		_serverModule?.Dispose();
	}
}

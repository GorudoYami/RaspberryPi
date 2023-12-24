using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Client.Models;

namespace RaspberryPi.Client.Tests;

[TestFixture]
public class ClientModuleTests {
	private ClientModule? _tcpClientModule;
	private Mock<IOptions<ClientModuleOptions>>? _mockedOptions;

	[SetUp]
	public void SetUp() {
		_mockedOptions = new Mock<IOptions<ClientModuleOptions>>();
	}

	private ClientModule GetInstance() {
		return _tcpClientModule ??= new ClientModule(_mockedOptions!.Object);
	}

	[Ignore("WIP")]
	[Test]
	public void Constructor() {
		_mockedOptions!.Setup(x => x.Value)
			.Returns(new ClientModuleOptions() {
				ServerHost = "localhost",
				ServerPort = 2137,
				TimeoutSeconds = 5000
			});

		Assert.DoesNotThrow(() => GetInstance());
	}

	[TearDown]
	public void TearDown() {
		_tcpClientModule?.Dispose();
	}
}

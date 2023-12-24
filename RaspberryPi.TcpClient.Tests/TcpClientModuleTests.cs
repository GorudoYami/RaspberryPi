using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.TcpClient.Models;

namespace RaspberryPi.TcpClient.Tests;

[TestFixture]
public class TcpClientModuleTests {
	private TcpClientModule? _tcpClientModule;
	private Mock<IOptions<TcpClientModuleOptions>>? _mockedOptions;

	[SetUp]
	public void SetUp() {
		_mockedOptions = new Mock<IOptions<TcpClientModuleOptions>>();
	}

	private TcpClientModule GetInstance() {
		return _tcpClientModule ??= new TcpClientModule(_mockedOptions!.Object);
	}

	[Ignore("WIP")]
	[Test]
	public void Constructor() {
		_mockedOptions!.Setup(x => x.Value)
			.Returns(new TcpClientModuleOptions() {
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

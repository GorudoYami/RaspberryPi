using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace RaspberryPi.TcpServer.Tests;

[TestFixture]
public class TcpServerModuleTests {
	private TcpServerModule? _tcpServerModule;
	private Mock<IOptions<TcpServerModuleOptions>>? _mockedOptions;

	[SetUp]
	public void SetUp() {
		_mockedOptions = new Mock<IOptions<TcpServerModuleOptions>>();
	}

	private TcpServerModule GetInstance() {
		return _tcpServerModule ??= new TcpServerModule(_mockedOptions!.Object);
	}

	[Ignore("WIP")]
	[Test]
	public void Constructor() {
		_mockedOptions!.Setup(x => x.Value)
			.Returns(new TcpServerModuleOptions() {
				Host = "localhost",
				Port = 2137
			});

		Assert.DoesNotThrow(() => GetInstance());
	}

	[TearDown]
	public void TearDown() {
		_tcpServerModule?.Dispose();
	}
}

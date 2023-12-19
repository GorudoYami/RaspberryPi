using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Modules.Models;

namespace RaspberryPi.Modules.Tests;

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
			.Returns(new TcpServerModuleOptions("localhost", 2137));

		Assert.DoesNotThrow(() => GetInstance());
	}

	[TearDown]
	public void TearDown() {
		_tcpServerModule?.Dispose();
	}
}

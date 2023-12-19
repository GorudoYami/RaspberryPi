using Moq;
using NUnit.Framework;
using RaspberryPi.Common.Modules;

namespace RaspberryPi.Modules.Tests;

[TestFixture]
public class RaspberryPiTests {
	private RaspberryPiModule? _raspberryPiModule;
	private Mock<ITcpClientModule>? _mockedTcpClientModule;

	[SetUp]
	public void SetUp() {
		_mockedTcpClientModule = new Mock<ITcpClientModule>();
	}

	private RaspberryPiModule GetInstance() {
		return _raspberryPiModule ??= new RaspberryPiModule(_mockedTcpClientModule!.Object);
	}

	[Ignore("WIP")]
	[Test]
	public void Constructor() {
		Assert.DoesNotThrow(() => GetInstance());
	}

	[TearDown]
	public void TearDown() {
		// If needed to dispose
	}
}

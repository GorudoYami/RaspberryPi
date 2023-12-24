using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Common.Modules;
using RaspberryPi.Modem.Models;

namespace RaspberryPi.Modem.Tests;

[TestFixture]
public class ModemModuleTests {
	private ModemModule? _modemModule;
	private Mock<IOptions<ModemModuleOptions>>? _mockedOptions;
	private Mock<ILogger<IModemModule>>? _mockedLogger;

	public ModemModuleTests() {

	}

	[SetUp]
	public void SetUp() {
		_mockedOptions = new Mock<IOptions<ModemModuleOptions>>();
		_mockedOptions.Setup(x => x.Value)
			.Returns(new ModemModuleOptions() { SerialPort = "xd" });
		_mockedLogger = new Mock<ILogger<IModemModule>>();

		_modemModule = null;
	}

	private ModemModule GetInstance() {
		return _modemModule ??= new ModemModule(_mockedOptions!.Object, _mockedLogger!.Object);
	}

	[Ignore("WIP")]
	[Test]
	public void Constructor() {
		Assert.DoesNotThrow(() => GetInstance());
	}

	[TearDown]
	public void TearDown() {
		//
	}
}

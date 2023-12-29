using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RaspberryPi.Common.Modules;
using RasperryPi.Common.Tests;

namespace RaspberryPi.Tests;

[TestFixture]
public class RaspberryPiTests {
	private RaspberryPiModule? _raspberryPiModule;

	private Mock<ILogger<IRaspberryPiModule>> _mockedLogger;
	private Mock<IClientModule> _mockedTcpClientModule;
	private Mock<IDrivingModule> _mockedDrivingModule;
	private Mock<IModemModule> _mockedModemModule;
	private Mock<ISensorsModule> _mockedSensorsModule;
	private Mock<ICameraModule> _mockedCameraModule;

	[SetUp]
	public void SetUp() {
		_mockedLogger = MockedLoggerProvider.GetMockedLogger<IRaspberryPiModule>();
		_mockedTcpClientModule = new Mock<IClientModule>();
		_mockedDrivingModule = new Mock<IDrivingModule>();
		_mockedCameraModule = new Mock<ICameraModule>();
		_mockedModemModule = new Mock<IModemModule>();
		_mockedSensorsModule = new Mock<ISensorsModule>();

		_raspberryPiModule = null;
	}

	private RaspberryPiModule GetInstance() {
		return _raspberryPiModule ??= new RaspberryPiModule(
			_mockedLogger.Object,
			_mockedTcpClientModule.Object,
			_mockedDrivingModule.Object,
			_mockedModemModule.Object,
			_mockedCameraModule.Object,
			_mockedSensorsModule.Object
		);
	}

	[Test]
	public void Constructor_DoesNotThrow() {
		Assert.DoesNotThrow(() => GetInstance());
	}
}

using GorudoYami.Common.Asynchronous;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Options;
using RasperryPi.Common.Tests;

namespace RaspberryPi.Tests;

[TestFixture]
public class RaspberryPiTests {
	private readonly RaspberryPiModuleOptions _options;
	private RaspberryPiModule? _raspberryPiModule;

	private Mock<IOptions<RaspberryPiModuleOptions>> _mockedOptions;
	private TestLogger<IRaspberryPiModule> _testLogger;
	private Mock<ICancellationTokenProvider> _mockedCancellationTokenProvider;
	private Mock<IClientProtocol> _mockedClientProtocol;
	private Mock<IClientModule> _mockedTcpClientModule;
	private Mock<IDrivingModule> _mockedDrivingModule;
	private Mock<IModemModule> _mockedModemModule;
	private Mock<ISensorsModule> _mockedSensorsModule;
	private Mock<ICameraModule> _mockedCameraModule;

	public RaspberryPiTests() {
		_options = new RaspberryPiModuleOptions() {
			PingTimeoutSeconds = 5,
			ReconnectPeriodSeconds = 15,
			DefaultSafety = true,
		};
	}

	[SetUp]
	public void SetUp() {
		_testLogger = new TestLogger<IRaspberryPiModule>();
		_mockedOptions = new Mock<IOptions<RaspberryPiModuleOptions>>();
		_mockedCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
		_mockedClientProtocol = new Mock<IClientProtocol>();
		_mockedTcpClientModule = new Mock<IClientModule>();
		_mockedDrivingModule = new Mock<IDrivingModule>();
		_mockedCameraModule = new Mock<ICameraModule>();
		_mockedModemModule = new Mock<IModemModule>();
		_mockedSensorsModule = new Mock<ISensorsModule>();

		_raspberryPiModule = null;
	}

	[TearDown]
	public void TearDown() {
		_testLogger.Dispose();
	}

	private RaspberryPiModule GetInstance() {
		return _raspberryPiModule ??= new RaspberryPiModule(
			_mockedOptions.Object,
			_testLogger,
			_mockedCancellationTokenProvider.Object,
			_mockedClientProtocol.Object,
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

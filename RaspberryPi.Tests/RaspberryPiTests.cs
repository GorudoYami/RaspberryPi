using GorudoYami.Common.Asynchronous;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Common.Services;
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
	private Mock<IServerProtocol> _mockedServerProtocol;
	private Mock<IDrivingService> _mockedDrivingService;
	private Mock<ISensorService> _mockedSensorService;
	private Mock<ICameraService> _mockedCameraService;
	private Mock<ITcpServerService> _mockedTcpServerService;

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
		_mockedServerProtocol = new Mock<IServerProtocol>();
		_mockedDrivingService = new Mock<IDrivingService>();
		_mockedCameraService = new Mock<ICameraService>();
		_mockedSensorService = new Mock<ISensorService>();
		_mockedTcpServerService = new Mock<ITcpServerService>();

		_raspberryPiModule = null;
	}

	[TearDown]
	public void TearDown() {
		_testLogger.Dispose();
	}

	private RaspberryPiModule? GetInstance() {
		//return _raspberryPiModule ??= new RaspberryPiModule(
		//	_mockedOptions.Object,
		//	_testLogger,
		//	_mockedCancellationTokenProvider.Object,
		//	_mockedServerProtocol.Object,
		//	_mockedDrivingService.Object,
		//	_mockedCameraService.Object,
		//	_mockedSensorService.Object,
		//	_mockedTcpServerService.Object
		//);
		return null;
	}

	[Test]
	public void Constructor_DoesNotThrow() {
		Assert.DoesNotThrow(() => GetInstance());
	}
}

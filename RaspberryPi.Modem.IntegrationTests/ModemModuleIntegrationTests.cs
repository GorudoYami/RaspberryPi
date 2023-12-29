using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Modem.Models;
using RaspberryPi.Server;
using RaspberryPi.Server.Models;
using RasperryPi.Common.Tests;
using System.Diagnostics;

namespace RaspberryPi.Modem.IntegrationTests;

[TestFixture]
public class ModemModuleIntegrationTests {
	private readonly ModemModuleOptions _modemOptions;
	private readonly ServerModuleOptions _serverOptions;

	private Mock<ILogger<IModemModule>> _mockedModemLogger;
	private Mock<ILogger<IServerModule>> _mockedServerLogger;
	private Mock<IOptions<ModemModuleOptions>> _mockedModemOptions;
	private Mock<IOptions<ServerModuleOptions>> _mockedServerOptions;
	private Mock<IClientProtocol> _mockedClientProtocol;
	private Mock<IServerProtocol> _mockedServerProtocol;

	private ServerModule? _serverModule;
	private ModemModule? _modemModule;

	public ModemModuleIntegrationTests() {
		_modemOptions = new ModemModuleOptions() {
			SerialPort = "COM7",
			TimeoutSeconds = 5,
			DefaultBaudRate = 9600,
			TargetBaudRate = 4000000,
			ServerHost = "93.176.248.32",
			ServerPort = 2137
		};

		_serverOptions = new ServerModuleOptions() {
			Host = "10.0.1.5",
			Port = 2137,
		};
	}

	[SetUp]
	public void SetUp() {
		_mockedModemOptions = new Mock<IOptions<ModemModuleOptions>>();
		_mockedModemOptions.Setup(x => x.Value).Returns(_modemOptions);
		_mockedServerOptions = new Mock<IOptions<ServerModuleOptions>>();
		_mockedServerOptions.Setup(x => x.Value).Returns(_serverOptions);
		_mockedModemLogger = MockedLoggerProvider.GetMockedLogger<IModemModule>();
		_mockedServerLogger = MockedLoggerProvider.GetMockedLogger<IServerModule>();
		_mockedClientProtocol = new Mock<IClientProtocol>();
		_mockedServerProtocol = new Mock<IServerProtocol>();
	}

	[TearDown]
	public void TearDown() {
		_modemModule?.Dispose();
		_serverModule?.Dispose();
		_serverModule = null;
		_modemModule = null;
	}

	private ServerModule GetServerInstance() {
		return _serverModule ??= new ServerModule(
			_mockedServerOptions.Object,
			_mockedServerLogger.Object,
			_mockedServerProtocol.Object
		);
	}

	private ModemModule GetInstance() {
		return _modemModule ??= new ModemModule(
			_mockedModemOptions.Object,
			_mockedModemLogger.Object,
			_mockedClientProtocol.Object
		);
	}

	[Test]
	public void Initialize_DoesNotThrow() {
		GetInstance();

		Assert.DoesNotThrowAsync(() => _modemModule!.InitializeAsync());
	}

	[Test]
	public async Task Start_ServerWorking_DoesNotThrow() {
		GetServerInstance();
		GetInstance();
		await _modemModule!.InitializeAsync();
		_serverModule!.Start();

		Assert.DoesNotThrowAsync(() => _modemModule.ConnectAsync());
	}
}

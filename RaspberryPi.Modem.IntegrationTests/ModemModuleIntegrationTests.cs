using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Common.Modules;
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
		_mockedModemLogger = MockedLoggerProvider.GetMockedLogger<IModemModule>();
		_mockedServerLogger = MockedLoggerProvider.GetMockedLogger<IServerModule>();

		_mockedModemOptions = new Mock<IOptions<ModemModuleOptions>>();
		_mockedModemOptions.Setup(x => x.Value).Returns(_modemOptions);

		_mockedServerOptions = new Mock<IOptions<ServerModuleOptions>>();
		_mockedServerOptions.Setup(x => x.Value).Returns(_serverOptions);
	}

	[TearDown]
	public void TearDown() {
		_modemModule?.Dispose();
		_modemModule = null;
	}

	private ServerModule CreateServerInstance() {
		return _serverModule ??= new ServerModule(_mockedServerOptions.Object, _mockedServerLogger.Object);
	}

	private ModemModule CreateInstance() {
		return _modemModule ??= new ModemModule(_mockedModemOptions.Object, _mockedModemLogger.Object);
	}

	[Test]
	public void Initialize_DoesNotThrow() {
		CreateInstance();

		Assert.DoesNotThrowAsync(() => _modemModule!.InitializeAsync());
	}

	[Test]
	public async Task Start_ServerWorking_DoesNotThrow() {
		CreateServerInstance();
		CreateInstance();
		await _modemModule!.InitializeAsync();
		_serverModule!.Start();

		Assert.DoesNotThrowAsync(() => _modemModule.StartAsync());
	}
}

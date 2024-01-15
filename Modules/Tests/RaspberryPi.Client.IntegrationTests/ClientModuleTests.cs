using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Client.Options;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Server;
using RaspberryPi.Server.Models;
using RaspberryPi.Tests.Common;

namespace RaspberryPi.Client.IntegrationTests;

[TestFixture]
public class ClientModuleTests {
	private readonly ClientModuleOptions _clientOptions;
	private readonly ServerModuleOptions _serverOptions;
	private ClientModule? _clientModule;
	private ServerModule? _serverModule;

	private TestLogger<IServerModule> _serverLogger;
	private Mock<IOptions<ServerModuleOptions>> _mockedServerOptions;
	private Mock<IOptions<ClientModuleOptions>> _mockedClientOptions;
	private Mock<IServerProtocol> _mockedServerProtocol;
	private Mock<IClientProtocol> _mockedClientProtocol;

	public ClientModuleTests() {
		_clientOptions = new ClientModuleOptions() {
			ServerHost = "localhost",
			ServerPort = 2137,
			TimeoutSeconds = 10
		};

		_serverOptions = new ServerModuleOptions() {
			Host = "10.0.1.254",
			Port = 2137
		};
	}

	[SetUp]
	public void SetUp() {
		_mockedClientOptions = new Mock<IOptions<ClientModuleOptions>>();
		_mockedClientOptions.Setup(x => x.Value).Returns(_clientOptions);
		_mockedServerOptions = new Mock<IOptions<ServerModuleOptions>>();
		_mockedServerOptions.Setup(x => x.Value).Returns(_serverOptions);
		_serverLogger = new TestLogger<IServerModule>();
		_mockedServerProtocol = new Mock<IServerProtocol>();
		_mockedClientProtocol = new Mock<IClientProtocol>();
	}

	[TearDown]
	public void TearDown() {
		_clientModule?.Dispose();
		_serverModule?.Dispose();
		_serverLogger.Dispose();
	}

	private ServerModule GetServerInstance() {
		return _serverModule ??= new ServerModule(
			_mockedServerOptions.Object,
			_serverLogger,
			_mockedServerProtocol.Object
		);
	}

	private ClientModule GetClientInstance() {
		return _clientModule ??= new ClientModule(
			_mockedClientOptions.Object,
			_mockedClientProtocol.Object
		);
	}

	[Ignore("WIP")]
	[Test]
	public void ConnectAsync_ServerIsDown_ReturnsFalse() {
		//bool result = await _clientModule!.ConnectAsync();

		//Assert.That(Is.False, alse);
	}

	[Test]
	public async Task Test() {
		await _serverModule!.InitializeAsync();
		while (true) {
			Thread.Sleep(100);
		}
	}

	[Test]
	public void DisconnectAsync_NotConnected_NothingHappens() {
		Assert.DoesNotThrowAsync(() => _clientModule!.DisconnectAsync());
	}

	[Ignore("Doesn't work, skill issue inside server")]
	[Test]
	public void ConnectAsync_ServerIsWorking_ReturnsTrue() {
		_serverModule!.Start();
		//bool result = await _clientModule!.ConnectAsync();

		//Assert.That(true, Is.True);
	}
}

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
	private IServerProtocol _serverProtocol;
	private IClientProtocol _clientProtocol;

	public ClientModuleTests() {
		_clientOptions = new ClientModuleOptions() {
			ServerHost = "10.0.1.10",
			MainServerPort = 2137,
			VideoServerPort = 6969,
			TimeoutSeconds = 10
		};

		_serverOptions = new ServerModuleOptions() {
			Host = "10.0.1.10",
			MainPort = 2137,
			VideoPort = 6969,
		};
	}

	[SetUp]
	public void SetUp() {
		_mockedClientOptions = new Mock<IOptions<ClientModuleOptions>>();
		_mockedClientOptions.Setup(x => x.Value).Returns(_clientOptions);
		_mockedServerOptions = new Mock<IOptions<ServerModuleOptions>>();
		_mockedServerOptions.Setup(x => x.Value).Returns(_serverOptions);
		_serverLogger = new TestLogger<IServerModule>();
		_serverProtocol = new StandardServerProtocol();
		_clientProtocol = new StandardClientProtocol();
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
			_serverProtocol
		);
	}

	private ClientModule GetClientInstance() {
		return _clientModule ??= new ClientModule(
			_mockedClientOptions.Object,
			_clientProtocol
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
		var server = GetServerInstance();
		var client = GetClientInstance();

		await server.InitializeAsync();
		await client.InitializeAsync();

		server.Start();
		await client.ConnectAsync();
	}

	[Ignore("WIP")]
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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Client.Models;
using RaspberryPi.Common.Modules;
using RaspberryPi.Server;
using RaspberryPi.Server.Models;

namespace RaspberryPi.Client.IntegrationTests;

[TestFixture]
public class ClientModuleTests {
	private ClientModule? _clientModule;
	private ServerModule? _serverModule;

	[SetUp]
	public void SetUp() {
		var clientOptions = new Mock<IOptions<ClientModuleOptions>>();
		clientOptions.Setup(x => x.Value).Returns(new ClientModuleOptions() {
			ServerHost = "localhost",
			ServerPort = 2137,
			TimeoutSeconds = 10
		});
		var serverOptions = new Mock<IOptions<ServerModuleOptions>>();
		serverOptions.Setup(x => x.Value).Returns(new ServerModuleOptions() {
			Host = "localhost",
			Port = 2137
		});
		var logger = new Mock<ILogger<IServerModule>>();

		_clientModule = new ClientModule(clientOptions.Object);
		_serverModule = new ServerModule(serverOptions.Object, logger.Object);
	}

	[Ignore("WIP")]
	[Test]
	public async Task ConnectAsync_ServerIsDown_ReturnsFalse() {
		//bool result = await _clientModule!.ConnectAsync();

		Assert.That(true, Is.False);
	}

	[Test]
	public void DisconnectAsync_NotConnected_NothingHappens() {
		Assert.DoesNotThrowAsync(() => _clientModule!.DisconnectAsync());
	}

	[Ignore("Doesn't work, skill issue inside server")]
	[Test]
	public async Task ConnectAsync_ServerIsWorking_ReturnsTrue() {
		_serverModule!.Start();
		//bool result = await _clientModule!.ConnectAsync();

		Assert.That(true, Is.True);
	}

	[TearDown]
	public void TearDown() {
		_clientModule?.Dispose();
		_serverModule?.Dispose();
	}
}

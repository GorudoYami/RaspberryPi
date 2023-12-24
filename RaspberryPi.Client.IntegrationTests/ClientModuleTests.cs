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
	private ClientModule? _tcpClientModule;
	private ServerModule? _tcpServerModule;

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

		_tcpClientModule = new ClientModule(clientOptions.Object);
		_tcpServerModule = new ServerModule(serverOptions.Object, logger.Object);
	}

	[Test]
	public async Task ConnectAsync_ServerIsDown_ReturnsFalse() {
		bool result = await _tcpClientModule!.ConnectAsync();

		Assert.That(result, Is.False);
	}

	[Test]
	public void DisconnectAsync_NotConnected_NothingHappens() {
		Assert.DoesNotThrowAsync(() => _tcpClientModule!.DisconnectAsync());
	}

	[Test]
	public async Task ConnectAsync_ServerIsWorking_ReturnsTrue() {
		_tcpServerModule!.Start();
		bool result = await _tcpClientModule!.ConnectAsync();

		Assert.That(result, Is.True);
	}

	[TearDown]
	public void TearDown() {
		_tcpClientModule?.Dispose();
		_tcpServerModule?.Dispose();
	}
}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Common.Modules;
using RaspberryPi.TcpClient.Models;
using RaspberryPi.TcpServer;
using RaspberryPi.TcpServer.Models;

namespace RaspberryPi.TcpClient.IntegrationTests;

[TestFixture]
public class TcpClientModuleTests {
	private TcpClientModule? _tcpClientModule;
	private TcpServerModule? _tcpServerModule;

	[SetUp]
	public void SetUp() {
		var clientOptions = new Mock<IOptions<TcpClientModuleOptions>>();
		clientOptions.Setup(x => x.Value).Returns(new TcpClientModuleOptions() {
			ServerHost = "localhost",
			ServerPort = 2137,
			TimeoutSeconds = 10
		});
		var serverOptions = new Mock<IOptions<TcpServerModuleOptions>>();
		serverOptions.Setup(x => x.Value).Returns(new TcpServerModuleOptions() {
			Host = "localhost",
			Port = 2137
		});
		var logger = new Mock<ILogger<ITcpServerModule>>();

		_tcpClientModule = new TcpClientModule(clientOptions.Object);
		_tcpServerModule = new TcpServerModule(serverOptions.Object, logger.Object);
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

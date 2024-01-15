using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Modem.Models;
using RaspberryPi.Modem.Options;
using RaspberryPi.Modem.Validators;
using RaspberryPi.Server;
using RaspberryPi.Server.Models;
using RasperryPi.Common.Tests;
using System.Diagnostics;

namespace RaspberryPi.Modem.IntegrationTests {
	[TestFixture]
	public class ModemModuleIntegrationTests {
		private readonly ModemModuleOptions _modemOptions;

		private Mock<IOptions<ModemModuleOptions>> _mockedModemOptions;
		private TestLogger<IModemModule> _modemLogger;
		private TestLogger<IResponseValidator> _responseValidatorLogger;

		private ModemModule? _modemModule;

		public ModemModuleIntegrationTests() {
			_modemOptions = new ModemModuleOptions() {
				SerialPort = "COM7",
				TimeoutSeconds = 5,
				DefaultBaudRate = 9600,
				TargetBaudRate = 4000000,
				ServerPort = 2137,
				ExpectedResponses = [
					new ExpectedResponse() {
						Command = "AT+CFUN=0",
						MatchAny = true,
						ResponseLines = ["OK", "+CPIN: NOT READY"]
					},
				]
			};
		}

		[SetUp]
		public void SetUp() {
			_mockedModemOptions = new Mock<IOptions<ModemModuleOptions>>();
			_mockedModemOptions.Setup(x => x.Value).Returns(_modemOptions);
			_modemLogger = new TestLogger<IModemModule>();
			_responseValidatorLogger = new TestLogger<IResponseValidator>();
		}

		[TearDown]
		public void TearDown() {
			_modemModule?.Dispose();
			_modemModule = null;

			_modemLogger.Dispose();
			_responseValidatorLogger.Dispose();
		}

		private ModemModule GetInstance() {
			return _modemModule ??= new ModemModule(
				_mockedModemOptions.Object,
				_modemLogger,
				new EncryptedClientProtocol(),
				new ResponseValidator(_mockedModemOptions.Object, _responseValidatorLogger)
			);
		}

		[Ignore("UwU")]
		[Test]
		public void Initialize_DoesNotThrow() {
			GetInstance();

			Assert.DoesNotThrowAsync(() => _modemModule!.InitializeAsync());
		}

		[Test]
		public async Task Start_ServerWorking_DoesNotThrow() {
			GetInstance();
			await _modemModule!.InitializeAsync();

			Assert.DoesNotThrowAsync(() => _modemModule.ConnectAsync());
		}
	}
}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Modem.Options;
using RaspberryPi.Modem.Validators;

namespace RaspberryPi.Modem.Tests {
	[TestFixture]
	public class ModemModuleTests {
		private readonly ModemModuleOptions _options;
		private ModemModule? _modemModule;

		private Mock<IOptions<ModemModuleOptions>> _mockedOptions;
		private Mock<ILogger<IModemModule>> _mockedLogger;
		private Mock<IClientProtocol> _mockedClientProtocol;
		private Mock<IResponseValidator> _mockedResponseValidator;

		public ModemModuleTests() {
			_options = new ModemModuleOptions() {
				DefaultBaudRate = 9600,
				TargetBaudRate = 4000000,
				SerialPort = "COM1",
				ServerPort = 2137,
				TimeoutSeconds = 5,
				ExpectedResponses = []
			};
		}

		[SetUp]
		public void SetUp() {
			_mockedOptions = new Mock<IOptions<ModemModuleOptions>>();
			_mockedOptions.Setup(x => x.Value).Returns(_options);
			_mockedLogger = new Mock<ILogger<IModemModule>>();
			_mockedClientProtocol = new Mock<IClientProtocol>();

			_modemModule = null;
		}

		private ModemModule GetInstance() {
			return _modemModule ??= new ModemModule(
				_mockedOptions.Object,
				_mockedLogger.Object,
				_mockedClientProtocol.Object,
				_mockedResponseValidator.Object
			);
		}

		[Ignore("WIP")]
		[Test]
		public void Constructor() {
			Assert.DoesNotThrow(() => GetInstance());
		}

		[TearDown]
		public void TearDown() {
			_modemModule?.Dispose();
		}
	}
}

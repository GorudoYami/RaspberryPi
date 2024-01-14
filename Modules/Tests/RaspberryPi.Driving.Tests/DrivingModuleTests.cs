using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Common.Gpio;
using RaspberryPi.Common.Gpio.Pwm;
using RaspberryPi.Common.Modules;
using RaspberryPi.Driving.Enums;
using RaspberryPi.Driving.Models;
using System.Device.Gpio;
using System.Diagnostics;

namespace RaspberryPi.Driving.Tests {
	public class DrivingModuleTests {
		private readonly List<DrivingPin> _pins;

		private DrivingModule? _drivingModule;
		private Mock<IOptions<DrivingModuleOptions>>? _mockedOptions;
		private Mock<IGpioControllerProvider>? _mockedController;
		private Mock<ILogger<IDrivingModule>>? _mockedLogger;

		public DrivingModuleTests() {
			_pins = [
				new(1, Direction.Left),
				new(2, Direction.Right),
				new(3, Direction.Forward),
				new(4, Direction.Back),
				new DrivingPwmPin(3, Direction.Forward | Direction.Back, 1, 400),
			];
		}

		[SetUp]
		public void SetUp() {
			_mockedOptions = new Mock<IOptions<DrivingModuleOptions>>();
			_mockedOptions.Setup(x => x.Value)
				.Returns(new DrivingModuleOptions() { Pins = _pins });

			_mockedController = new Mock<IGpioControllerProvider>();
			_mockedController!.Setup(x => x.GetPwmChannel(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<double>()))
				.Returns(new Mock<IPwmChannelProvider>().Object);

			_mockedLogger = new Mock<ILogger<IDrivingModule>>();
			_mockedLogger.Setup(x => x.Log(
				It.IsAny<LogLevel>(),
				It.IsAny<EventId>(),
				It.IsAny<object>(),
				It.IsAny<Exception?>(),
				It.IsAny<Func<object, Exception?, string>>()))
				.Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>((logLevel, eventId, state, exception, formatter)
					=> Debug.WriteLine(formatter?.Invoke(state, exception)));

			_drivingModule = null;
		}

		private DrivingModule GetInstance() {
			return _drivingModule ??= new DrivingModule(_mockedOptions!.Object, _mockedLogger!.Object, _mockedController!.Object);
		}

		[Test]
		public void Constructor_InitializesPins() {
			DrivingPwmPin pwmPin = _pins.OfType<DrivingPwmPin>().Single();

			GetInstance();

			foreach (DrivingPin pin in _pins.Where(x => x is not DrivingPwmPin)) {
				_mockedController!
					.Verify(x => x.OpenPin(pin.Number, PinMode.Output), Times.Once);
			}

			_mockedController!
				.Verify(x => x.GetPwmChannel(pwmPin.Chip, pwmPin.Number, pwmPin.Frequency, 0), Times.Once);
		}

		[Test]
		public void Dispose_DeinitializePins() {
			DrivingPwmPin pwmPin = _pins.OfType<DrivingPwmPin>().Single();
			var mockedChannel = new Mock<IPwmChannelProvider>();
			_mockedController!.Setup(x => x.GetPwmChannel(pwmPin.Chip, pwmPin.Number, pwmPin.Frequency, 0))
				.Returns(mockedChannel.Object);

			DrivingModule instance = GetInstance();
			instance.Dispose();

			foreach (DrivingPin pin in _pins.Where(x => x is not DrivingPwmPin)) {
				_mockedController!
					.Verify(x => x.ClosePin(pin.Number), Times.Once);
			}

			mockedChannel.Verify(x => x.Stop(), Times.AtLeastOnce);
			mockedChannel.Verify(x => x.Dispose(), Times.Once);
		}

		[TearDown]
		public void TearDown() {
			_drivingModule?.Dispose();
		}
	}
}

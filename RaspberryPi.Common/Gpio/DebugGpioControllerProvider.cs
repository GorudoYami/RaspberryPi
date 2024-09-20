using Microsoft.Extensions.Logging;
using RaspberryPi.Common.Gpio.Pwm;
using System;
using System.Collections.Generic;
using System.Device;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Gpio {
	public class DebugGpioControllerProvider
		: IGpioControllerProvider {
		private readonly ILogger<IGpioControllerProvider> _logger;

		public DebugGpioControllerProvider(ILogger<IGpioControllerProvider> logger) {
			_logger = logger;
		}

		private sealed class DebugPin {
			public PinMode Mode { get; }
			public PinValue Value { get; set; }

			public DebugPin(PinMode mode = PinMode.Output, PinValue? value = null) {
				Mode = mode;
				Value = value ?? PinValue.Low;
			}
		}

		private readonly Dictionary<int, DebugPin> _pins = new Dictionary<int, DebugPin>();

		public void ClosePin(int pinNumber) {
			throw new NotImplementedException();
		}

		public PinMode GetPinMode(int pinNumber) {
			throw new NotImplementedException();
		}

		public IPwmChannelProvider GetPwmChannel(int chip, int channel, int frequency, double dutyCyclePercentage) {
			return new DebugPwmChannelProvider(chip, channel, frequency, dutyCyclePercentage);
		}

		public bool IsPinModeSupported(int pinNumber, PinMode mode) {
			throw new NotImplementedException();
		}

		public void OpenPin(int pinNumber) {
			if (_pins.ContainsKey(pinNumber)) {
				throw new InvalidOperationException($"Pin {pinNumber} already open");
			}

			_logger.LogDebug("Opening pin {PinNumber}", pinNumber);
			_pins[pinNumber] = new DebugPin();
		}

		public void OpenPin(int pinNumber, PinMode pinMode) {
			if (_pins.ContainsKey(pinNumber)) {
				throw new InvalidOperationException($"Pin {pinNumber} already open");
			}

			_logger.LogDebug("Opening pin {PinNumber} with mode {PinMode}", pinNumber, pinMode);
			_pins[pinNumber] = new DebugPin(pinMode);
		}

		public void OpenPin(int pinNumber, PinMode pinMode, PinValue initialValue) {
			if (_pins.ContainsKey(pinNumber)) {
				throw new InvalidOperationException($"Pin {pinNumber} already open");
			}

			_logger.LogDebug("Opening pin {PinNumber} with mode {PinMode}, initial value {PinInitialValue}", pinNumber, pinMode, initialValue);
			_pins[pinNumber] = new DebugPin(pinMode, initialValue);
		}

		public ComponentInformation QueryComponentInformation() {
			throw new NotImplementedException();
		}

		public PinValue Read(int pinNumber) {
			if (_pins.ContainsKey(pinNumber) == false) {
				throw new InvalidOperationException($"Pin {pinNumber} not open");
			}
			else if (_pins[pinNumber].Mode != PinMode.Input) {
				throw new InvalidOperationException($"Pin {pinNumber} not in Input mode");
			}

			PinValue pinValue = _pins[pinNumber].Value;
			_logger.LogTrace("Reading {PinValue} from pin {PinNumber}", pinValue, pinNumber);
			return pinValue;
		}

		public void SetPinMode(int pinNumber, PinMode mode) {
			throw new NotImplementedException();
		}

		public void Subscribe(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback) {
			throw new NotImplementedException();
		}

		public void Toggle(int pinNumber) {
			throw new NotImplementedException();
		}

		public void Unsubscribe(int pinNumber, PinChangeEventHandler callback) {
			throw new NotImplementedException();
		}

		public WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public void Write(ReadOnlySpan<PinValuePair> pinValuePairs) {
			foreach (var pinValuePair in pinValuePairs) {
				_pins[pinValuePair.PinNumber].Value = pinValuePair.PinValue;
				_logger.LogTrace("Writing {PinValue} to {PinNumber}", pinValuePair.PinValue, pinValuePair.PinNumber);
			}
		}

		public void Write(int pinNumber, PinValue value) {
			_pins[pinNumber].Value = value;
			_logger.LogTrace("Writing {PinValue} to {PinNumber}", value, pinNumber);
		}
	}
}

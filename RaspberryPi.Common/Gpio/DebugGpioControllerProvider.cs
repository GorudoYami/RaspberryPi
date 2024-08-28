using Microsoft.Extensions.Logging;
using RaspberryPi.Common.Gpio.Pwm;
using System;
using System.Collections.Generic;
using System.Device;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Gpio;

public class DebugGpioControllerProvider(ILogger<IGpioControllerProvider> logger)
	: IGpioControllerProvider {
	private class DebugPin(PinMode mode = PinMode.Output, PinValue? value = null) {
		public PinMode Mode { get; } = mode;
		public PinValue Value { get; set; } = value ?? PinValue.Low;
	}

	private readonly Dictionary<int, DebugPin> _pins = [];

	public void ClosePin(int pinNumber) {
		throw new NotImplementedException();
	}

	public PinMode GetPinMode(int pinNumber) {
		throw new NotImplementedException();
	}

	public IPwmChannelProvider GetPwmChannel(int chip, int channel, int frequency, double dutyCyclePercentage) {
		throw new NotImplementedException();
	}

	public bool IsPinModeSupported(int pinNumber, PinMode mode) {
		throw new NotImplementedException();
	}

	public IGpioPinProvider OpenPin(int pinNumber) {
		if (_pins.ContainsKey(pinNumber)) {
			throw new InvalidOperationException($"Pin {pinNumber} already open");
		}

		logger.LogDebug("Opening pin {PinNumber}", pinNumber);
		_pins[pinNumber] = new DebugPin();
		return new DebugGpioPinProvider();
	}

	public IGpioPinProvider OpenPin(int pinNumber, PinMode pinMode) {
		if (_pins.ContainsKey(pinNumber)) {
			throw new InvalidOperationException($"Pin {pinNumber} already open");
		}

		logger.LogDebug("Opening pin {PinNumber} with mode {PinMode}", pinNumber, pinMode);
		_pins[pinNumber] = new DebugPin(pinMode);
		return new DebugGpioPinProvider();
	}

	public IGpioPinProvider OpenPin(int pinNumber, PinMode pinMode, PinValue initialValue) {
		if (_pins.ContainsKey(pinNumber)) {
			throw new InvalidOperationException($"Pin {pinNumber} already open");
		}

		logger.LogDebug("Opening pin {PinNumber} with mode {PinMode}, initial value {PinInitialValue}", pinNumber, pinMode, initialValue);
		_pins[pinNumber] = new DebugPin(pinMode, initialValue);
		return new DebugGpioPinProvider();
	}

	public ComponentInformation QueryComponentInformation() {
		throw new NotImplementedException();
	}

	public PinValue Read(int pinNumber) {
		if (_pins.ContainsKey(pinNumber)) {
			throw new InvalidOperationException($"Pin {pinNumber} not open");
		}
		else if (_pins[pinNumber].Mode != PinMode.Input) {
			throw new InvalidOperationException($"Pin {pinNumber} not in Input mode");
		}

		return _pins[pinNumber].Value;
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
		throw new NotImplementedException();
	}

	public void Write(int pinNumber, PinValue value) {
		throw new NotImplementedException();
	}
}

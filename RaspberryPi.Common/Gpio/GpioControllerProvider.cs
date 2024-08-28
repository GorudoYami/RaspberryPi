using RaspberryPi.Common.Gpio.Pwm;
using System;
using System.Device;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Gpio;

public class GpioControllerProvider : IGpioControllerProvider, IDisposable {
	public int PinCount => _controller.PinCount;
	public PinNumberingScheme NumberingScheme => _controller.NumberingScheme;

	private readonly GpioController _controller;

	public GpioControllerProvider() {
		_controller = new GpioController();
	}

	public PinValue Read(int pinNumber) {
		return _controller.Read(pinNumber);
	}

	public void Read(Span<PinValuePair> pinValuePairs) {
		_controller.Read(pinValuePairs);
	}

	public void Write(ReadOnlySpan<PinValuePair> pinValuePairs) {
		_controller.Write(pinValuePairs);
	}

	public void Write(int pinNumber, PinValue value) {
		_controller.Write(pinNumber, value);
	}

	public IGpioPinProvider OpenPin(int pinNumber) {
		return new GpioPinProvider();
	}

	public IGpioPinProvider OpenPin(int pinNumber, PinMode pinMode) {
		return new GpioPinProvider();
	}

	public IGpioPinProvider OpenPin(int pinNumber, PinMode pinMode, PinValue initialValue) {
		return new GpioPinProvider();
	}

	public void ClosePin(int pinNumber) {
		_controller.ClosePin(pinNumber);
	}

	public PinMode GetPinMode(int pinNumber) {
		return _controller.GetPinMode(pinNumber);
	}

	public void Toggle(int pinNumber) {
		_controller.Toggle(pinNumber);
	}

	public bool IsPinOpen(int pinNumber) {
		return _controller.IsPinOpen(pinNumber);
	}

	public bool IsPinModeSupported(int pinNumber, PinMode mode) {
		return _controller.IsPinModeSupported(pinNumber, mode);
	}

	public ComponentInformation QueryComponentInformation() {
		return _controller.QueryComponentInformation();
	}

	public void Subscribe(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback) {
		_controller.RegisterCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);
	}

	public void Unsubscribe(int pinNumber, PinChangeEventHandler callback) {
		_controller.UnregisterCallbackForPinValueChangedEvent(pinNumber, callback);
	}

	public void SetPinMode(int pinNumber, PinMode mode) {
		_controller.SetPinMode(pinNumber, mode);
	}

	public WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken = default) {
		return _controller.WaitForEvent(pinNumber, eventTypes, cancellationToken);
	}

	public ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken = default) {
		return _controller.WaitForEventAsync(pinNumber, eventTypes, cancellationToken);
	}

	public IPwmChannelProvider GetPwmChannel(int chip, int channel, int frequency, double dutyCyclePercentage) {
		return new PwmChannelProvider(chip, channel, frequency, dutyCyclePercentage);
	}

	public void Dispose() {
		GC.SuppressFinalize(this);
		_controller.Dispose();
	}
}

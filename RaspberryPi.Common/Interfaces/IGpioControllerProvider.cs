using System;
using System.Collections.Generic;
using System.Device;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Interfaces;

public interface IGpioControllerProvider {
	void ClosePin(int pinNumber);
	PinMode GetPinMode(int pinNumber);
	bool IsPinModeSupported(int pinNumber, PinMode mode);
	GpioPin OpenPin(int pinNumber);
	GpioPin OpenPin(int pinNumber, PinMode pinMode);
	GpioPin OpenPin(int pinNumber, PinMode pinMode, PinValue initialValue);
	ComponentInformation QueryComponentInformation();
	PinValue Read(int pinNumber);
	void Read(Span<PinValuePair> pinValuePairs);
	void SetPinMode(int pinNumber, PinMode mode);
	void Subscribe(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback);
	void Toggle(int pinNumber);
	void Unsubscribe(int pinNumber, PinChangeEventHandler callback);
	WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken = default);
	ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken = default);
	void Write(ReadOnlySpan<PinValuePair> pinValuePairs);
	void Write(int pinNumber, PinValue value);
}

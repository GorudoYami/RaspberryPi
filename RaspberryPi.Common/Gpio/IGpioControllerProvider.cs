﻿using RaspberryPi.Common.Gpio.Pwm;
using System;
using System.Device;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Gpio {
	public interface IGpioControllerProvider {
		void ClosePin(int pinNumber);
		PinMode GetPinMode(int pinNumber);
		IPwmChannelProvider GetPwmChannel(int chip, int channel, int frequency, double dutyCyclePercentage);
		bool IsPinModeSupported(int pinNumber, PinMode mode);
		void OpenPin(int pinNumber);
		void OpenPin(int pinNumber, PinMode pinMode);
		void OpenPin(int pinNumber, PinMode pinMode, PinValue initialValue);
		ComponentInformation QueryComponentInformation();
		PinValue Read(int pinNumber);
		void SetPinMode(int pinNumber, PinMode mode);
		void Subscribe(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback);
		void Toggle(int pinNumber);
		void Unsubscribe(int pinNumber, PinChangeEventHandler callback);
		WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken = default);
		ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken = default);
		void Write(ReadOnlySpan<PinValuePair> pinValuePairs);
		void Write(int pinNumber, PinValue value);
	}
}

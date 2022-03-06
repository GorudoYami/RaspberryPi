using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Linq;
using RaspberryPi.Drivers;
using RaspberryPi.Drivers.Enums;

namespace RaspberryPi.Drivers {
	public class CarDriver : IDisposable {
		public int TurnPower { get; private set; }
		public int DrivePower { get; private set; }
		public Direction? TurnDirection { get; private set; }
		public Direction? DriveDirection { get; private set; }
		public bool IsMoving { get; private set; }
		public List<CarDriverPin> Pins { get; private set; }
		public List<CarDriverPwmPin> PwmPins { get; private set;}

		private GpioController Controller { get; set; }

		public CarDriver(List<CarDriverPin> pins, List<CarDriverPwmPin> pwmPins) {
			TurnPower = 0;
			DriveDirection = null;
			IsMoving = false;
			InitializePins(pins, pwmPins);
		}

		private void InitializePins(List<CarDriverPin> pins, List<CarDriverPwmPin> pwmPins) {
			Pins = pins;

			foreach (var pin in Pins) {
				Controller.OpenPin(pin.Key, pin.Mode);
				pin.Open = true;
			}
		}

		private void DeinitializePins() {
			foreach (var pin in Pins) {
				Controller.ClosePin(pin.Key);
				pin.Open = false;
			}
		}

		public void Left(int power = 50) {
			TurnDirection = Direction.Left;
			TurnPower = power;
			Update();
		}

		public void Right(int power = 50) {
			TurnDirection = Direction.Right;
			TurnPower = power;
			Update();
		}

		public void Straight(int power = 100) {
			DriveDirection = Direction.Straight;
			DrivePower = power;
			Update();
		}

		public void Back(int power = 100) {
			DriveDirection = Direction.Back;
			DrivePower = power;
			Update();
		}

		public void Stop() {
			DriveDirection = null;
			TurnDirection = null;
			DrivePower = 0;
			TurnPower = 0;
			Update();
		}

		private void Update() {
			if (TurnDirection is not null) {
				CarDriverPin pin = Pins.Find(p => p.BoundDirection == TurnDirection);
				Controller.Write(pin.Key, PinValue.High);
			}
		}

		public void Dispose() {
			Stop();
			DeinitializePins();
			Controller.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Timers;
using RaspberryPi.Drivers.Enums;

namespace RaspberryPi.Driver {
	public class CarDriver : IDisposable {
		public int TurnPower { get; private set; }
		public int DrivePower { get; private set; }
		public Direction? TurnDirection { get; private set; }
		public Direction? DriveDirection { get; private set; }
		public bool IsMoving { get; private set; }
		public Dictionary<int, PinMode> PinsUsed { get; private set; }

		private GpioController Controller { get; set; }

		public CarDriver(Dictionary<int, PinMode> pins) {
			TurnPower = 0;
			DriveDirection = null;
			IsMoving = false;
			Controller = new GpioController();
			InitializePins(pins);
		}

		private void InitializePins(Dictionary<int, PinMode> pins) {
			PinsUsed = pins;

			foreach (var pin in PinsUsed)
				Controller.OpenPin(pin.Key, pin.Value);
		}

		private void DeinitializePins() {
			foreach (var pin in PinsUsed)
				Controller.ClosePin(pin.Key);
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

		}

		public void Dispose() {
			Stop();
			DeinitializePins();
			Controller.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Linq;
using RaspberryPi.Drivers;
using RaspberryPi.Drivers.Enums;

namespace RaspberryPi.Drivers;

public class CarDriver : IDisposable {
	public int TurnPower { get; private set; }
	public int DrivePower { get; private set; }
	public Direction? TurnDirection { get; private set; }
	public Direction? DriveDirection { get; private set; }
	public bool IsMoving { get; private set; }
	public List<IDriverPin> Pins { get; private set; }

	private GpioController Controller { get; set; }
	private Dictionary<Direction, PwmChannel> PwmChannels { get; set; }

	public CarDriver(List<IDriverPin> pins) {
		TurnPower = 0;
		DrivePower = 0;
		TurnDirection = null;
		DriveDirection = null;
		IsMoving = false;
		Pins = pins;
		Controller = new GpioController();
		PwmChannels = new Dictionary<Direction, PwmChannel>();

		if (!ValidatePins())
			throw new ArgumentException("Pin list does not contain a pin for every direction", nameof(pins));

		InitializePins();
	}

	private bool ValidatePins() {
		foreach (var direction in Enum.GetValues<Direction>()) {
			if (!Pins.Exists(p => p.BoundDirection == direction))
				return false;
		}

		if (!Pins.Exists(p => p is CarDriverPwmPin && p.BoundDirection is Direction.Left or Direction.Right))
			return false;

		if (!Pins.Exists(p => p is CarDriverPwmPin && p.BoundDirection is Direction.Forward or Direction.Back))
			return false;

		return true;
	}

	private void InitializePins() {
		foreach (var pin in Pins) {
			if (pin is CarDriverPin) {
				Controller.OpenPin(pin.Key, pin.Mode);
				pin.Open = true;
			}
			else if (pin is CarDriverPwmPin pwmPin) {
				PwmChannel pwmChannel = PwmChannel.Create(pwmPin.PwmChip, pwmPin.Key);
				PwmChannels.Add(pin.BoundDirection, pwmChannel);
			}
		}
	}

	private void DeinitializePins() {
		foreach (var pin in Pins) {
			Controller.ClosePin(pin.Key);
			pin.Open = false;
		}
	}

	public void Left(int power = 50) {
		if (TurnDirection is Direction.Left && TurnPower == power)
			return;

		TurnDirection = Direction.Left;
		TurnPower = power;
		UpdateTurnPins();
	}

	public void Right(int power = 50) {
		if (TurnDirection is Direction.Right && TurnPower == power)
			return;

		TurnDirection = Direction.Right;
		TurnPower = power;
		UpdateTurnPins();
	}

	public void Straight() {
		TurnDirection = null;
		UpdateTurnPins();
	}

	public void Forward(int power = 100) {
		DriveDirection = Direction.Forward;
		DrivePower = power;
		UpdateDrivePins();
	}

	public void Back(int power = 100) {
		if (DriveDirection is Direction.Back && DrivePower == power)
			return;

		DriveDirection = Direction.Back;
		DrivePower = power;
		UpdateDrivePins();
	}

	public void Stop() {
		DriveDirection = null;
		TurnDirection = null;
		DrivePower = 0;
		TurnPower = 0;
		IsMoving = false;
		UpdateDrivePins();
		UpdateTurnPins();
	}

	private void UpdateTurnPins() {
		if (TurnDirection is null)
			SetTurnPinsLow();
		else
			ToggleTurnPins();
	}

	private void SetTurnPinsLow() {
		IDriverPin pin = Pins.Single(p => p.BoundDirection == Direction.Left);
		Controller.Write(pin.Key, PinValue.Low);

		pin = Pins.Single(p => p.BoundDirection == Direction.Right);
		Controller.Write(pin.Key, PinValue.Low);

		PwmChannel pwmChannel = PwmChannels.Single(x => x.Key is Direction.Left or Direction.Right).Value;
		pwmChannel.Stop();
	}

	private void ToggleTurnPins() {
		IDriverPin pin = Pins.Single(p => p.BoundDirection == GetOppositeDirection(TurnDirection.Value));
		Controller.Write(pin.Key, PinValue.Low);

		PwmChannel pwmChannel = PwmChannels.Single(x => x.Key is Direction.Left or Direction.Right).Value;
		pwmChannel.Stop();
		pwmChannel.DutyCycle = TurnPower;
		pwmChannel.Start();

		pin = Pins.Single(p => p.BoundDirection == TurnDirection);
		Controller.Write(pin.Key, PinValue.High);
	}

	private void UpdateDrivePins() {
		if (DriveDirection is null)
			SetDrivePinsLow();
		else {
			ToggleDrivePins();
			IsMoving = true;
		}
	}

	private void SetDrivePinsLow() {
		IDriverPin pin = Pins.Single(p => p.BoundDirection == Direction.Forward);
		Controller.Write(pin.Key, PinValue.Low);

		pin = Pins.Single(p => p.BoundDirection == Direction.Back);
		Controller.Write(pin.Key, PinValue.Low);

		PwmChannel pwmChannel = PwmChannels.Single(x => x.Key is Direction.Forward or Direction.Back).Value;
		pwmChannel.Stop();
	}

	private void ToggleDrivePins() {
		IDriverPin pin = Pins.Single(p => p.BoundDirection == GetOppositeDirection(DriveDirection.Value));
		Controller.Write(pin.Key, PinValue.Low);

		PwmChannel pwmChannel = PwmChannels.Single(x => x.Key is Direction.Forward or Direction.Back).Value;
		pwmChannel.Stop();
		pwmChannel.DutyCycle = DrivePower;
		pwmChannel.Start();

		pin = Pins.Single(p => p.BoundDirection == DriveDirection);
		Controller.Write(pin.Key, PinValue.High);
	}

	private static Direction GetOppositeDirection(Direction direction) {
		return direction switch {
			Direction.Left => Direction.Right,
			Direction.Right => Direction.Left,
			Direction.Forward => Direction.Back,
			Direction.Back => Direction.Forward,
			_ => throw new ArgumentException("Enum Direction does not contain specified value", nameof(direction))
		};
	}

	public void Dispose() {
		Stop();
		DeinitializePins();
		Controller.Dispose();
		GC.SuppressFinalize(this);
	}
}

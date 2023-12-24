using Microsoft.Extensions.Options;
using RaspberryPi.Common.Interfaces;
using RaspberryPi.Common.Modules;
using RaspberryPi.Modules.Enums;
using RaspberryPi.Modules.Models;
using System.Device.Gpio;
using System.Device.Pwm;

namespace RaspberryPi.Modules;

public class DrivingModule : ICarModule, IDisposable {
	private readonly ICollection<IDrivingPin> _pins;
	private readonly IGpioControllerProvider _controller;
	private readonly Dictionary<Direction, PwmChannel> _pwmChannels;
	private int _turnPower;
	private int _drivePower;
	private Direction? _turnDirection;
	private Direction? _driveDirection;

	public DrivingModule(IOptions<DrivingModuleOptions> options, IGpioControllerProvider controller) {
		_pins = options.Value.Pins;
		_turnPower = 0;
		_drivePower = 0;
		_turnDirection = null;
		_driveDirection = null;
		_controller = controller;
		_pwmChannels = [];

		InitializePins();
	}

	private void InitializePins() {
		foreach (IDrivingPin pin in _pins) {
			if (pin is Pin) {
				_controller.OpenPin(pin.Number, PinMode.Output);
			}
			else if (pin is PwmPin pwmPin) {
				PwmChannel pwmChannel = PwmChannel.Create(pwmPin.PwmChip, pwmPin.Number, dutyCyclePercentage: 0);
				_pwmChannels.Add(pin.Direction, pwmChannel);
			}
		}
	}

	private void DeinitializePins() {
		foreach (Pin pin in _pins.OfType<Pin>()) {
			_controller.ClosePin(pin.Number);
		}
		foreach (PwmChannel pwmChannel in _pwmChannels.Values) {
			pwmChannel.Stop();
			pwmChannel.Dispose();
		}
	}

	public void Left(int power = 50) {
		if (_turnDirection is Direction.Left && _turnPower == power) {
			return;
		}

		_turnDirection = Direction.Left;
		_turnPower = power;
		UpdateTurnPins();
	}

	public void Right(int power = 50) {
		if (_turnDirection is Direction.Right && _turnPower == power) {
			return;
		}

		_turnDirection = Direction.Right;
		_turnPower = power;
		UpdateTurnPins();
	}

	public void Straight() {
		_turnDirection = null;
		UpdateTurnPins();
	}

	public void Forward(int power = 100) {
		_driveDirection = Direction.Forward;
		_drivePower = power;
		UpdateDrivePins();
	}

	public void Back(int power = 100) {
		if (_driveDirection is Direction.Back && _drivePower == power) {
			return;
		}

		_driveDirection = Direction.Back;
		_drivePower = power;
		UpdateDrivePins();
	}

	public void Stop() {
		_driveDirection = null;
		_turnDirection = null;
		_drivePower = 0;
		_turnPower = 0;
		UpdateDrivePins();
		UpdateTurnPins();
	}

	private void UpdateTurnPins() {
		if (_turnDirection == null) {
			SetTurnPinsLow();
		}
		else {
			ToggleTurnPins();
		}
	}

	private void SetTurnPinsLow() {
		IDrivingPin pin = _pins.Single(p => p.Direction == Direction.Left);
		_controller.Write(pin.Number, PinValue.Low);

		pin = _pins.Single(p => p.Direction == Direction.Right);
		_controller.Write(pin.Number, PinValue.Low);

		PwmChannel pwmChannel = _pwmChannels.Single(x => x.Key is Direction.Left or Direction.Right).Value;
		pwmChannel.Stop();
	}

	private void ToggleTurnPins() {
		IDrivingPin pin = _pins.Single(p => p.Direction == GetOppositeDirection(_turnDirection!.Value));
		_controller.Write(pin.Number, PinValue.Low);

		PwmChannel pwmChannel = _pwmChannels.Single(x => x.Key is Direction.Left or Direction.Right).Value;
		pwmChannel.Stop();
		pwmChannel.DutyCycle = _turnPower;
		pwmChannel.Start();

		pin = _pins.Single(p => p.Direction == _turnDirection);
		_controller.Write(pin.Number, PinValue.High);
	}

	private void UpdateDrivePins() {
		if (_driveDirection == null) {
			SetDrivePinsLow();
		}
		else {
			ToggleDrivePins();
		}
	}

	private void SetDrivePinsLow() {
		IDrivingPin pin = _pins.Single(p => p.Direction == Direction.Forward);
		_controller.Write(pin.Number, PinValue.Low);

		pin = _pins.Single(p => p.Direction == Direction.Back);
		_controller.Write(pin.Number, PinValue.Low);

		PwmChannel pwmChannel = _pwmChannels.Single(x => x.Key is Direction.Forward or Direction.Back).Value;
		pwmChannel.Stop();
	}

	private void ToggleDrivePins() {
		IDrivingPin pin = _pins.Single(p => p.Direction == GetOppositeDirection(_driveDirection!.Value));
		_controller.Write(pin.Number, PinValue.Low);

		PwmChannel pwmChannel = _pwmChannels.Single(x => x.Key is Direction.Forward or Direction.Back).Value;
		pwmChannel.Stop();
		pwmChannel.DutyCycle = _drivePower;
		pwmChannel.Start();

		pin = _pins.Single(p => p.Direction == _driveDirection);
		_controller.Write(pin.Number, PinValue.High);
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
		GC.SuppressFinalize(this);
		Stop();
		DeinitializePins();
	}
}

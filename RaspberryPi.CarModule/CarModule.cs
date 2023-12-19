using Microsoft.Extensions.Options;
using RaspberryPi.CarModule.Enums;
using RaspberryPi.CarModule.Models;
using RaspberryPi.Common.Modules;
using System.Device.Gpio;
using System.Device.Pwm;

namespace RaspberryPi.CarModule;

public class CarModule : ICarModule, IDisposable {
	private readonly List<IDriverPin> _pins;
	private int _turnPower;
	private int _drivePower;
	private Direction? _turnDirection;
	private Direction? _driveDirection;

	private GpioController Controller { get; set; }
	private Dictionary<Direction, PwmChannel> PwmChannels { get; set; }

	public CarModule(IOptions<CarModuleOptions> options) {
		_pins = options.Value.Pins.ToList();
		_turnPower = 0;
		_drivePower = 0;
		_turnDirection = null;
		_driveDirection = null;
		Controller = new GpioController();
		PwmChannels = new Dictionary<Direction, PwmChannel>();

		ValidatePins();
		InitializePins();
	}

	private bool ValidatePins() {
		bool missingDirectionPins = Enum.GetValues<Direction>()
			.Except(_pins.Select(x => x.BoundDirection), EqualityComparer<Direction>.Default)
			.Any();

		foreach (var direction in Enum.GetValues<Direction>()) {
			if (_pins.Exists(p => p.BoundDirection == direction) == false)
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

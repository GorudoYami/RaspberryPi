using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Common.Interfaces;
using RaspberryPi.Driving.Models;

namespace RaspberryPi.Driving.Tests;

public class DrivingModuleTests {
	private DrivingModule? _drivingModule;
	private Mock<IOptions<DrivingModuleOptions>>? _mockedOptions;
	private Mock<IGpioControllerProvider>? _mockedController;

	[SetUp]
	public void SetUp() {
		_mockedOptions = new Mock<IOptions<DrivingModuleOptions>>();
		_mockedController = new Mock<IGpioControllerProvider>();
	}

	private DrivingModule GetInstance() {
		return _drivingModule ??= new DrivingModule(_mockedOptions!.Object, _mockedController!.Object);
	}

	[Ignore("WIP")]
	[Test]
	public void Constructor_IncompletePins_ThrowException() {
		_mockedOptions!.Setup(x => x.Value)
			.Returns(new DrivingModuleOptions() { Pins = new List<DrivingPin>() });

		Assert.That(false, Is.True);
	}

	[TearDown]
	public void TearDown() {
		_drivingModule?.Dispose();
	}
}

using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Modules.Models;

namespace RaspberryPi.Modules.Tests;

public class DrivingModuleTests {
	private DrivingModule? _drivingModule;
	private Mock<IOptions<DrivingModuleOptions>>? _mockedOptions;

	[SetUp]
	public void SetUp() {
		_mockedOptions = new Mock<IOptions<DrivingModuleOptions>>();
	}

	private DrivingModule GetInstance() {
		return _drivingModule ??= new DrivingModule(_mockedOptions!.Object);
	}

	[Ignore("WIP")]
	[Test]
	public void Constructor_IncompletePins_ThrowException() {
		_mockedOptions!.Setup(x => x.Value)
			.Returns(new DrivingModuleOptions() { Pins = new List<IPin>() });

		Assert.That(false, Is.True);
	}

	[TearDown]
	public void TearDown() {
		_drivingModule?.Dispose();
	}
}

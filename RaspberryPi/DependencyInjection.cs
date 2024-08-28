using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaspberryPi.Camera;
using RaspberryPi.Camera.Options;
using RaspberryPi.Common.Extensions;
using RaspberryPi.Common.Options;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Common.Providers;
using RaspberryPi.Common.Services;
using RaspberryPi.Driving;
using RaspberryPi.Driving.Options;
using RaspberryPi.Options;
using RaspberryPi.Sensors;
using RaspberryPi.Sensors.Options;
using RaspberryPi.TcpServer;
using System;

namespace RaspberryPi;
public static class DependencyInjection {
	private static bool IsDebug() {
		return Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Debug";
	}

	public static IServiceCollection AddResolvers(this IServiceCollection services) {
		if (IsDebug()) {
			return services
				.AddSingleton<IVideoDeviceProvider, DebugVideoDeviceProvider>();
		}
		else {
			return services
				.AddSingleton<IVideoDeviceProvider, VideoDeviceProvider>();
		}
	}

	public static IServiceCollection AddProtocols(this IServiceCollection services) {
		return services
			.AddSingleton<ICommunicationProtocol, CommunicationProtocol>();
	}

	public static IServiceCollection AddServices(this IServiceCollection services) {
		return services
			.AddModule<IRaspberryPiModule, RaspberryPiModule>()
			.AddModule<ICameraService, CameraService>()
			.AddModule<ISensorService, SensorService>()
			.AddModule<IDrivingService, DrivingService>()
			.AddModule<ITcpServerService, TcpServerService>();
	}

	public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration) {
		services
			.AddOptions<RaspberryPiModuleOptions>()
			.Bind(configuration.GetRequiredSection(nameof(RaspberryPiModuleOptions)))
			.Validate(RaspberryPiModuleOptions.Validate)
			.ValidateOnStart();

		services
			.AddOptions<CameraServiceOptions>()
			.Bind(configuration.GetRequiredSection(nameof(CameraServiceOptions)))
			.ValidateOnStart();

		services
			.AddOptions<SensorsServiceOptions>()
			.Bind(configuration.GetRequiredSection(nameof(SensorsServiceOptions)))
			.Validate(SensorsServiceOptions.Validate)
			.ValidateOnStart();

		services
			.AddOptions<DrivingServiceOptions>()
			.Bind(configuration.GetRequiredSection(nameof(DrivingServiceOptions)))
			.Validate(DrivingServiceOptions.Validate)
			.ValidateOnStart();

		services
			.AddOptions<VideoDeviceOptions>()
			.Bind(configuration.GetRequiredSection(nameof(VideoDeviceOptions)))
			.ValidateOnStart();

		return services;
	}
}

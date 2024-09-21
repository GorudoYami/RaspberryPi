using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaspberryPi.Camera;
using RaspberryPi.Camera.Options;
using RaspberryPi.Common.Gpio;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Common.Providers;
using RaspberryPi.Common.Services;
using RaspberryPi.Common.Utilities;
using RaspberryPi.Driving;
using RaspberryPi.Driving.Options;
using RaspberryPi.Options;
using RaspberryPi.Sensors;
using RaspberryPi.Sensors.Options;
using RaspberryPi.TcpServer;
using RaspberryPi.TcpServer.Options;
using System;

namespace RaspberryPi {
	public static class DependencyInjection {
		private static bool IsDebug() {
			return Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")?.Equals("DEBUG", StringComparison.OrdinalIgnoreCase) ?? false;
		}

		public static IServiceCollection AddProviders(this IServiceCollection services) {
			if (IsDebug()) {
				return services
					.AddSingleton<IVideoDeviceProvider, DebugVideoDeviceProvider>()
					.AddSingleton<ICancellationTokenProvider, CancellationTokenProvider>()
					.AddSingleton<IGpioControllerProvider, DebugGpioControllerProvider>();
			}
			else {
				return services
					.AddSingleton<IVideoDeviceProvider, VideoDeviceProvider>()
					.AddSingleton<ICancellationTokenProvider, CancellationTokenProvider>()
					.AddSingleton<IGpioControllerProvider, GpioControllerProvider>();
			}
		}

		public static IServiceCollection AddProtocols(this IServiceCollection services) {
			return services
				.AddSingleton<ICommunicationProtocol, CommunicationProtocol>();
		}

		public static IServiceCollection AddServices(this IServiceCollection services) {
			return services
				.AddSingleton<IRaspberryPiModule, RaspberryPiModule>()
				.AddSingleton<ICameraService, CameraService>()
				.AddSingleton<ISensorService, SensorService>()
				.AddSingleton<IService>(x => x.GetRequiredService<ISensorService>())
				.AddSingleton<IDrivingService, DrivingService>()
				.AddSingleton<IService>(x => x.GetRequiredService<IDrivingService>())
				.AddSingleton<ITcpServerService, TcpServerService>();
		}

		public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration) {
			services
				.AddOptions<RaspberryPiOptions>()
				.Bind(configuration.GetRequiredSection(nameof(RaspberryPiOptions)))
				.ValidateOnStart();

			services
				.AddOptions<TcpServerOptions>()
				.Bind(configuration.GetRequiredSection(nameof(TcpServerOptions)))
				.ValidateOnStart();

			services
				.AddOptions<SensorOptions>()
				.Bind(configuration.GetRequiredSection(nameof(SensorOptions)))
				.Validate(SensorOptions.Validate)
				.ValidateOnStart();

			services
				.AddOptions<DrivingOptions>()
				.Bind(configuration.GetRequiredSection(nameof(DrivingOptions)))
				.Validate(DrivingOptions.Validate)
				.ValidateOnStart();

			services
				.AddOptions<VideoDeviceOptions>()
				.Bind(configuration.GetRequiredSection(nameof(VideoDeviceOptions)))
				.ValidateOnStart();

			return services;
		}
	}
}

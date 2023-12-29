using GorudoYami.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using RaspberryPi.Camera;
using RaspberryPi.Client;
using RaspberryPi.Client.Models;
using RaspberryPi.Common.Modules;
using RaspberryPi.Driving;
using RaspberryPi.Driving.Models;
using RaspberryPi.Modem;
using RaspberryPi.Modem.Models;
using RaspberryPi.Sensors;
using RaspberryPi.Sensors.Models;

namespace RaspberryPi;

public static class Program {
	public static void Main() {
		using ServiceProvider serviceProvide = CreateServiceProvider();
		IRaspberryPiModule raspberryPi = serviceProvide.GetRequiredService<IRaspberryPiModule>();

		raspberryPi.RunAsync().GetAwaiter().GetResult();
	}

	private static ServiceProvider CreateServiceProvider() {
		IConfiguration configuration = new ConfigurationBuilder()
			.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
			.AddWritableAppSettings(reloadOnChange: true)
			.Build();

		IServiceCollection services = new ServiceCollection()
			.AddSingleton(configuration)
			.AddModules()
			.AddOptions()
			.AddLogging(builder => {
				builder.ClearProviders();
				builder.SetMinimumLevel(LogLevel.Debug);
				builder.AddNLog();
			});

		return services.BuildServiceProvider();
	}

	private static IServiceCollection AddModules(this IServiceCollection services) {
		return services
			.AddModule<IRaspberryPiModule, RaspberryPiModule>()
			.AddModule<ICameraModule, CameraModule>()
			.AddModule<ISensorsModule, SensorsModule>()
			.AddModule<IModemModule, ModemModule>()
			.AddModule<IDrivingModule, DrivingModule>()
			.AddModule<IClientModule, ClientModule>();
	}

	public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration) {
		services
			.AddOptions<ClientModuleOptions>()
			.Bind(configuration.GetRequiredSection(nameof(ClientModuleOptions)))
			.Validate(ClientModuleOptions.Validate)
			.ValidateOnStart();

		services
			.AddOptions<ModemModuleOptions>()
			.Bind(configuration.GetRequiredSection(nameof(ModemModuleOptions)))
			.Validate(ModemModuleOptions.Validate)
			.ValidateOnStart();

		services
			.AddOptions<DrivingModuleOptions>()
			.Bind(configuration.GetRequiredSection(nameof(DrivingModuleOptions)))
			.Validate(DrivingModuleOptions.Validate)
			.ValidateOnStart();

		services
			.AddOptions<SensorsModuleOptions>()
			.Bind(configuration.GetRequiredSection(nameof(SensorsModuleOptions)))
			.Validate(SensorsModuleOptions.Validate)
			.ValidateOnStart();

		return services;
	}
}

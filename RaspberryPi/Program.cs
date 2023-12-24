using GorudoYami.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using RaspberryPi.Client;
using RaspberryPi.Client.Models;
using RaspberryPi.Common.Modules;

namespace RaspberryPi;

public static class Program {
	public static void Main() {
		using ServiceProvider serviceProvide = CreateServiceProvider();
		IRaspberryPiModule raspberryPi = serviceProvide.GetRequiredService<IRaspberryPiModule>();

		raspberryPi.Run();
	}

	public static ServiceProvider CreateServiceProvider() {
		IConfiguration configuration = new ConfigurationBuilder()
			.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
			.AddWritableAppSettings(reloadOnChange: true)
			.Build();

		IServiceCollection serviceCollection = new ServiceCollection()
			.AddSingleton(configuration)
			.AddModule<IRaspberryPiModule, RaspberryPiModule>()
			.AddModule<IClientModule, ClientModule>()
			.AddLogging(builder => {
				builder.ClearProviders();
				builder.SetMinimumLevel(LogLevel.Debug);
				builder.AddNLog();
			});

		serviceCollection
			.AddOptions<ClientModuleOptions>()
			.Bind(configuration.GetRequiredSection(nameof(ClientModuleOptions)));

		return serviceCollection.BuildServiceProvider();
	}
}

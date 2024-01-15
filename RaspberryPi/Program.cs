using GorudoYami.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using RaspberryPi.Common.Modules;
using System;

namespace RaspberryPi {
	public static class Program {
		public static void Main() {
			using (ServiceProvider serviceProvide = CreateServiceProvider()) {
				IRaspberryPiModule raspberryPi = serviceProvide.GetRequiredService<IRaspberryPiModule>();

				raspberryPi.RunAsync().GetAwaiter().GetResult();
			}
		}

		private static ServiceProvider CreateServiceProvider() {
			IConfiguration configuration = new ConfigurationBuilder()
				.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
				.AddWritableAppSettings(reloadOnChange: true)
				.Build();

			IServiceCollection services = new ServiceCollection()
				.AddSingleton(configuration)
				.AddProtocols()
				.AddModules()
				.AddOptions()
				.AddLogging(builder => {
					builder.ClearProviders();
					builder.SetMinimumLevel(LogLevel.Debug);
					builder.AddNLog();
				});

			return services.BuildServiceProvider();
		}
	}
}

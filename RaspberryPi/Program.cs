﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using RaspberryPi.Common.Extensions;
using RaspberryPi.Common.Services;
using System;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace RaspberryPi {
	public static class Program {
		public static void Main() {
			try {
				InitializeNlog();

				using (ServiceProvider serviceProvide = CreateServiceProvider()) {
					IRaspberryPiModule raspberryPi = serviceProvide.GetRequiredService<IRaspberryPiModule>();

					raspberryPi.RunAsync().GetAwaiter().GetResult();
				}
			}
			finally {
				DeinitializeNlog();
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
				.AddServices()
				.AddOptions(configuration)
				.AddProviders()
				.AddLogging(builder => {
					builder.ClearProviders();
					builder.SetMinimumLevel(LogLevel.Trace);
					builder.AddNLog(configuration);
				});

			return services.BuildServiceProvider();
		}

		private static void InitializeNlog() {
			LogManager.ThrowExceptions = true;
			LogManager.ThrowConfigExceptions = true;
			LogManager
				.Setup()
				.LoadConfigurationFromFile("nlog.config");
		}

		private static void DeinitializeNlog() {
			LogManager.Shutdown();
		}
	}
}

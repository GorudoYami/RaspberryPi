using RaspberryPi.Common.Configuration;
using Microsoft.Extensions.Configuration;

namespace RaspberryPi.Common.Extensions {
	public static class ConfigurationBuilderExtensions {
		public static IConfigurationBuilder AddWritableAppSettings(this IConfigurationBuilder builder, string filePath = "appsettings.json", bool optional = false, bool reloadOnChange = false) {
			var source = new JsonWritableConfigurationSource() {
				FileProvider = null,
				Path = filePath,
				ReloadOnChange = reloadOnChange,
				Optional = optional
			};

			source.ResolveFileProvider();

			return builder.Add(source);
		}
	}
}

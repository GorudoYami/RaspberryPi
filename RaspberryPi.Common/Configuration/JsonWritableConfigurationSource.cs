using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace RaspberryPi.Common.Configuration {
	public class JsonWritableConfigurationSource : JsonConfigurationSource {
		public override IConfigurationProvider Build(IConfigurationBuilder builder) {
			EnsureDefaults(builder);
			return new JsonWritableConfigurationProvider(this);
		}
	}
}

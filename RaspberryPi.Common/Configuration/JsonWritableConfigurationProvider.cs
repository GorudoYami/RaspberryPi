using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RaspberryPi.Common.Configuration {
	public class JsonWritableConfigurationProvider : JsonConfigurationProvider {
		public JsonWritableConfigurationProvider(JsonConfigurationSource source)
			: base(source) {

		}
		public override void Set(string key, string value) {
			base.Set(key, value);
			Save();
		}

		private void Save() {
			string json = Serialize();
			File.WriteAllText(Source.Path, json);
		}

		private static readonly char[] separator = new char[] { ':' };

		private string Serialize() {
			JsonNode rootNode = new JsonObject();

			foreach (KeyValuePair<string, string> kv in Data) {
				var keys = new Stack<string>(kv.Key.Split(separator, StringSplitOptions.RemoveEmptyEntries).Reverse());
				JsonNode node = rootNode;

				while (keys.Count > 1) {
					string key = keys.Pop();
					JsonNode nextNode = node[key];

					if (nextNode == null) {
						nextNode = new JsonObject();
						node[key] = nextNode;
						node = nextNode;
					}
					else {
						node = nextNode;
					}
				}

				string lastKey = keys.Pop();
				node[lastKey] = kv.Value;
			}

			return rootNode.ToJsonString(new JsonSerializerOptions() {
				WriteIndented = true
			});
		}
	}
}

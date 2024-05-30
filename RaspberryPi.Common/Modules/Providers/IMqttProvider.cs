using GorudoYami.Common.Modules;
using RaspberryPi.Common.Models.Mqtt;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Modules.Providers {
	public interface IMqttProvider : IModule {
		IReadOnlyDictionary<string, MqttTopic> Topics { get; }

		string GetTopicValue(string topicName);
		Task PublishAsync(string topicName, string value, CancellationToken cancellationToken = default);
		Task SubscribeAsync(string topicName, CancellationToken cancellationToken = default);
		Task UnsubscribeAsync(string topicName, CancellationToken cancellationToken = default);
	}
}

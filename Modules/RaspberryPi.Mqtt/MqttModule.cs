using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using RaspberryPi.Common.Exceptions;
using RaspberryPi.Common.Models.Mqtt;
using RaspberryPi.Common.Modules;
using RaspberryPi.Mqtt.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Mqtt {
	public class MqttModule : IMqttModule, IDisposable, IAsyncDisposable {
		public bool Enabled => _options.Enabled;
		public bool IsInitialized { get; private set; }
		public bool Connected => _client?.IsConnected ?? false;
		public IReadOnlyDictionary<string, MqttTopic> Topics => _topics;

		private readonly MqttModuleOptions _options;
		private readonly ILogger<IMqttModule> _logger;
		private readonly MqttFactory _factory;
		private readonly Dictionary<string, MqttTopic> _topics;
		private IMqttClient _client;

		public MqttModule(IOptions<MqttModuleOptions> options, ILogger<IMqttModule> logger) {
			_topics = new Dictionary<string, MqttTopic>();
			_options = options.Value;
			_logger = logger;
			_factory = new MqttFactory();
		}

		public Task InitializeAsync(CancellationToken cancellationToken = default) {
			return Task.CompletedTask;
		}

		public async Task ConnectAsync(CancellationToken cancellationToken = default) {
			if (Connected) {
				_logger.LogWarning("Client was already connected - disconnecting.");
				await DisconnectAsync(cancellationToken);
			}

			_client = _factory.CreateMqttClient();
			MqttClientConnectResult result = await _client.ConnectAsync(
				new MqttClientOptionsBuilder()
				.WithTcpServer(_options.ServerHost, _options.ServerPort)
				.WithTlsOptions(options => options.WithCertificateValidationHandler(_ => true))
				.Build(),
				cancellationToken
			);

			if (result.ResultCode != MqttClientConnectResultCode.Success) {
				await DisconnectAsync(cancellationToken);
				throw new InitializeCommunicationException(
					$"Could not connect to MQTT server at {_options.ServerHost}:{_options.ServerPort}. Result {result.ResultCode}"
				);
			}

			_client.ApplicationMessageReceivedAsync += MessageReceivedAsync;
		}

		private Task MessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e) {
			return Task.Run(() => {
				MqttTopic topic = Topics[e.ApplicationMessage.Topic];
				string value = e.ApplicationMessage.ConvertPayloadToString();
				topic.UpdateValue(value);
			});
		}

		public async Task DisconnectAsync(CancellationToken cancellationToken = default) {
			if (_client != null) {
				await _client.DisconnectAsync(
					new MqttClientDisconnectOptionsBuilder()
					.WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection)
					.Build(),
					cancellationToken
				);
			}

			_client?.Dispose();
			_client = null;
		}

		public async Task SubscribeAsync(string topicName, CancellationToken cancellationToken = default) {
			AssertConnected();

			_topics.Add(topicName, new MqttTopic(topicName, null));
			await _client.SubscribeAsync(topicName, cancellationToken: cancellationToken);
		}

		public async Task UnsubscribeAsync(string topicName, CancellationToken cancellationToken = default) {
			AssertConnected();

			if (_topics.ContainsKey(topicName) == false) {
				throw new InvalidOperationException($"Topic {topicName} is not subscribed");
			}

			await _client.UnsubscribeAsync(topicName, cancellationToken);
			_topics.Remove(topicName);
		}

		public async Task PublishAsync(string topicName, string value, CancellationToken cancellationToken = default) {
			AssertConnected();

			await _client.PublishStringAsync(topicName, value, cancellationToken: cancellationToken);
		}

		public string GetTopicValue(string topicName) {
			return Topics.TryGetValue(topicName, out MqttTopic value) ? value.Value : null;
		}

		private void AssertConnected() {
			if (Connected == false) {
				throw new InvalidOperationException("Not connected to a server");
			}
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			DisconnectAsync().GetAwaiter().GetResult();
		}

		public async ValueTask DisposeAsync() {
			GC.SuppressFinalize(this);
			await DisconnectAsync();
		}
	}
}

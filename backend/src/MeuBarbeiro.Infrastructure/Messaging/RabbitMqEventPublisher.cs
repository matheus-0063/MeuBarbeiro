using System.Text;
using System.Text.Json;
using MeuBarbeiro.Application.Abstractions.Messaging;
using MeuBarbeiro.Contracts.Events;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MeuBarbeiro.Infrastructure.Messaging;

public sealed class RabbitMqEventPublisher(IRabbitMqConnectionProvider connectionProvider, RabbitMqTopologyInitializer topologyInitializer,
    IOptions<RabbitMqOptions> options) : IEventPublisher
{
    private readonly RabbitMqOptions _options = options.Value;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        topologyInitializer.Initialize();

        using var channel = connectionProvider.CreateChannel();

        var routingKey = ResolveRoutingKey(typeof(TMessage));
        var payload = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.MessageId = Guid.NewGuid().ToString("N");
        properties.Type = typeof(TMessage).Name;
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        properties.ContentType = "application/json";

        channel.BasicPublish(
            exchange: _options.Exchange,
            routingKey: routingKey,
            basicProperties: properties,
            body: payload);

        return Task.CompletedTask;
    }

    private string ResolveRoutingKey(Type messageType)
    {
        if (messageType == typeof(AppointmentRequestedIntegrationEvent))
        {
            return _options.RequestedQueue;
        }

        if (messageType == typeof(AppointmentStatusUpdatedIntegrationEvent))
        {
            return _options.StatusUpdatedQueue;
        }

        throw new InvalidOperationException($"Nao existe routing key configurada para {messageType.Name}.");
    }
}

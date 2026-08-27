using System.Text.Json;
using MeuBarbeiro.Application.Abstractions.Messaging;
using MeuBarbeiro.Contracts.Events;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MeuBarbeiro.Infrastructure.Messaging;

public sealed class RabbitMqEventPublisher(
    IRabbitMqConnectionProvider connectionProvider,
    RabbitMqTopologyInitializer topologyInitializer,
    IOptions<RabbitMqOptions> options) : IEventPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RabbitMqOptions _options = options.Value;

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
            _options.Exchange,
            routingKey,
            properties,
            payload);

        return Task.CompletedTask;
    }

    private string ResolveRoutingKey(Type messageType)
    {
        if (messageType == typeof(AppointmentRequestedIntegrationEvent)) return _options.RequestedQueue;

        if (messageType == typeof(AppointmentStatusUpdatedIntegrationEvent)) return _options.StatusUpdatedQueue;

        throw new InvalidOperationException($"Nao existe routing key configurada para {messageType.Name}.");
    }
}
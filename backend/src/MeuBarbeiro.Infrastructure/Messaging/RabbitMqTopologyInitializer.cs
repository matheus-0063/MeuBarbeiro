using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MeuBarbeiro.Infrastructure.Messaging;

public sealed class RabbitMqTopologyInitializer(
    IRabbitMqConnectionProvider connectionProvider,
    IOptions<RabbitMqOptions> options)
{
    private readonly RabbitMqOptions _options = options.Value;

    public void Initialize()
    {
        using var channel = connectionProvider.CreateChannel();

        channel.ExchangeDeclare(
            exchange: _options.Exchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        channel.QueueDeclare(
            queue: _options.RequestedQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        channel.QueueBind(
            queue: _options.RequestedQueue,
            exchange: _options.Exchange,
            routingKey: _options.RequestedQueue);

        channel.QueueDeclare(
            queue: _options.StatusUpdatedQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        channel.QueueBind(
            queue: _options.StatusUpdatedQueue,
            exchange: _options.Exchange,
            routingKey: _options.StatusUpdatedQueue);
    }
}

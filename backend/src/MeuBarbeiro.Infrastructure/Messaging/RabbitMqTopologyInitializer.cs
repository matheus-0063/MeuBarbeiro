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
            _options.Exchange,
            ExchangeType.Direct,
            true,
            false);

        channel.QueueDeclare(
            _options.RequestedQueue,
            true,
            false,
            false,
            null);

        channel.QueueBind(
            _options.RequestedQueue,
            _options.Exchange,
            _options.RequestedQueue);

        channel.QueueDeclare(
            _options.StatusUpdatedQueue,
            true,
            false,
            false,
            null);

        channel.QueueBind(
            _options.StatusUpdatedQueue,
            _options.Exchange,
            _options.StatusUpdatedQueue);
    }
}
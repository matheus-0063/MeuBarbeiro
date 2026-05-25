using RabbitMQ.Client;

namespace MeuBarbeiro.Infrastructure.Messaging;

public interface IRabbitMqConnectionProvider : IDisposable
{
    IConnection GetConnection();
    IModel CreateChannel();
}

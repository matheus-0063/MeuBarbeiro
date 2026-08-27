using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MeuBarbeiro.Infrastructure.Messaging;

public sealed class RabbitMqConnectionProvider(IOptions<RabbitMqOptions> options) : IRabbitMqConnectionProvider
{
    private readonly RabbitMqOptions _options = options.Value;
    private readonly object _syncRoot = new();
    private IConnection? _connection;
    private bool _disposed;

    public IConnection GetConnection()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RabbitMqConnectionProvider));

        if (_connection is { IsOpen: true }) return _connection;

        lock (_syncRoot)
        {
            if (_connection is { IsOpen: true }) return _connection;

            _connection?.Dispose();

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection("MeuBarbeiro");
            return _connection;
        }
    }

    public IModel CreateChannel()
    {
        return GetConnection().CreateModel();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _connection?.Dispose();
        _disposed = true;
    }
}
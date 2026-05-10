namespace MeuBarbeiro.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string Exchange { get; set; } = "meu-barbeiro.events";
    public string RequestedQueue { get; set; } = "appointments.requested";
    public string StatusUpdatedQueue { get; set; } = "appointments.status-updated";
}

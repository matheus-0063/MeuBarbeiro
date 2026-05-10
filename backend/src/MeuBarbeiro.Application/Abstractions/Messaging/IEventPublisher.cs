namespace MeuBarbeiro.Application.Abstractions.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default);
}

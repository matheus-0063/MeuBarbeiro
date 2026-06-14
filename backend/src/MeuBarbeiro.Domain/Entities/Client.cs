namespace MeuBarbeiro.Domain.Entities;

public class Client
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    public Client() { }

    public Client(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
    }
}
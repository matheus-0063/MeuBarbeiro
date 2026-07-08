namespace MeuBarbeiro.Domain.Entities;

public class Client
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    private Client() { }

    public Client(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId é obrigatório.", nameof(userId));

        Id = Guid.NewGuid();
        UserId = userId;
    }
}
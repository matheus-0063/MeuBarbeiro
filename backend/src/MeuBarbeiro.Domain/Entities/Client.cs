namespace MeuBarbeiro.Domain.Entities;

public class Client
{
    public Client()
    {
    }

    public Client(Guid userId)
    {
        ValidarUserId(userId);

        Id = Guid.NewGuid();
        UserId = userId;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    private static void ValidarUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId é obrigatório", nameof(userId));
    }
}
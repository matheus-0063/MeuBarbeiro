namespace MeuBarbeiro.Domain.Entities;

public class Client
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    public Client() { }

    public Client(Guid userId)
    {
        ValidarUserId(userId);

        Id = Guid.NewGuid();
        UserId = userId;
    }
    
    private static void ValidarUserId(Guid userId)
    {
        if (userId == Guid.Empty) 
            throw new ArgumentException("UserId é obrigatório", nameof(userId));
    } 
}
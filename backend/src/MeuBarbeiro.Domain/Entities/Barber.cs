namespace MeuBarbeiro.Domain.Entities;

public class Barber
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? BarbershopId { get; private set; }

    public Barber() { }

    public Barber(Guid userId, Guid barberId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        BarbershopId = barberId;
    }
}
namespace MeuBarbeiro.Domain.Entities;

public class Barber
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? BarbershopId { get; private set; }

    public Barber() { }

    public Barber(Guid userId, Guid? barbershopId = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        BarbershopId = barbershopId;
    }

    public void AssignBarbershop(Guid barbershopId)
    {
        BarbershopId = barbershopId;
    }
}

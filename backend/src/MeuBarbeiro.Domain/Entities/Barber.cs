namespace MeuBarbeiro.Domain.Entities;

public class Barber
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? BarbershopId { get; private set; }

    public Barber() { }

    public Barber(Guid userId, Guid? barbershopId = null)
    {
        ValidarUserId(userId);
        
        Id = Guid.NewGuid();
        UserId = userId;
        BarbershopId = barbershopId;
    }

    public void AssignBarbershop(Guid barbershopId)
    {
        BarbershopId = barbershopId;
    }

    private static void ValidarUserId(Guid userId)
    {
        if (userId == Guid.Empty) 
            throw new ArgumentException("UserId é obrigatório", nameof(userId));
    } 
}

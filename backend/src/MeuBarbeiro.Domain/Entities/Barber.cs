using MeuBarbeiro.Domain.Exceptions;

namespace MeuBarbeiro.Domain.Entities;

public class Barber
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? BarbershopId { get; private set; }

    public Barber() { }

    public Barber(Guid userId)
    {
        ValidateUserId(userId);
        
        Id = Guid.NewGuid();
        UserId = userId;
    }

    public void AssignBarbershop(Guid barbershopId)
    {
        ValidateBarberId(barbershopId);

        if (BarbershopId != null && BarbershopId != Guid.Empty) 
            throw new BarberBelongsAnotherBarbershopException();
            
        BarbershopId = barbershopId;
    }
    
    public void RemoveFromBarbershop(Guid barbershopId)
    {
        ValidateBarberId(barbershopId);
        
        if (barbershopId != BarbershopId) 
            throw new BarberDoesNotBelongBarbershopException(barbershopId);
        
        BarbershopId = null;
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty) 
            throw new ArgumentException("UserId é obrigatório", nameof(userId));
    }

    private static void ValidateBarberId(Guid barberId)
    {
        if (barberId == Guid.Empty) throw new ArgumentException(null, nameof(barberId));
    }
}

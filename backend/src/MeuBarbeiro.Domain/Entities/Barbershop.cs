namespace MeuBarbeiro.Domain.Entities;

public sealed class Barbershop
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerUserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public double AverageRating { get; private set; }

    public Barbershop() { }
    
    public Barbershop(Guid ownerUserId, string name, string city, string address, string description)
    {
        EnsureOwnedBy(ownerUserId);
        
        Id = Guid.NewGuid();
        OwnerUserId = ownerUserId;
        Name = name;
        City = city;
        Address = address;
        Description = description;
    }
    
    public void UpdateDetails(string name, string city, string address, string description)
    {
        Name = name;
        City = city;
        Address = address;
        Description = description;
    }

    public void UpdateAverageRating(double averageRating)
    {
        if (averageRating is < 0 or > 5)
            throw new ArgumentOutOfRangeException(nameof(averageRating), "A avaliação média deve estar entre 0 e 5.");
        
        AverageRating = averageRating;
    }

    private static void EnsureOwnedBy(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty) throw new ArgumentException(null, nameof(ownerUserId));
    }
}

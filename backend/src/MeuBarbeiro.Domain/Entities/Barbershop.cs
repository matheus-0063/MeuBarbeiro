namespace MeuBarbeiro.Domain.Entities;

public sealed class Barbershop
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double AverageRating { get; set; }

    public Barbershop() { }
    
    public Barbershop(string name, string city, string address, string description)
    {
        Id = Guid.NewGuid();
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
}

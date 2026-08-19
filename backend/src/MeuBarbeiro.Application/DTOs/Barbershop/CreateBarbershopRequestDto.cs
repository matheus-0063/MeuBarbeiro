namespace MeuBarbeiro.Application.DTOs.Barbershop;

public class CreateBarbershopRequestDto
{
    public Guid OwnerUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

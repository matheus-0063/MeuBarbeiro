namespace MeuBarbeiro.Application.DTOs.Barbers;

public class RegisterBarberRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
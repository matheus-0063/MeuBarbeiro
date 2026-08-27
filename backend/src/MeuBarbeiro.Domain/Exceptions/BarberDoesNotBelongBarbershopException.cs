namespace MeuBarbeiro.Domain.Exceptions;

public class BarberDoesNotBelongBarbershopException(Guid barbershopId)
    : DomainException($"Barbeiro não pertence a barbearia de ID: {barbershopId}.")
{
}
namespace MeuBarbeiro.Domain.Exceptions;

public class ServiceOfferingValidationException(string propertyName, string message) : DomainException(message)
{
    public string PropertyName => propertyName;
}
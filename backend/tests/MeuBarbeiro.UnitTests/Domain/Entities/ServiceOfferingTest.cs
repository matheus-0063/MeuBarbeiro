using FluentAssertions;
using MeuBarbeiro.Domain.Exceptions;
using MeuBarbeiro.UnitTests.TestBuilder;

namespace MeuBarbeiro.UnitTests.Domain.Entities;

public class ServiceOfferingTest
{
    [Fact]
    public void ServiceOffering_DeveGerarId_QuandoForCriado()
    {
        // Arrange
        var serviceOffering = new ServiceOfferingBuilder()
            .WithName("Corte")
            .WithDescription("Corte na preferencia do cliente")
            .WithPrice(50.0m)
            .WithDurationMinutes(30)
            .Build();

        // Assert
        serviceOffering.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void ServiceOffering_DeveFalhar_QuandoBarbershopIdForEmpty()
    {
        // Arrange 
        var func = () => new ServiceOfferingBuilder()
            .WithBarbershopId(Guid.Empty)
            .Build();

        // Assert
        func.Should().Throw<ServiceOfferingValidationException>();
    }
}
using FluentAssertions;
using MeuBarbeiro.UnitTests.TestBuilder;

namespace MeuBarbeiro.UnitTests.Domain.Entities;

public class ServiceOfferingTest
{
    [Fact]
    public void ServiceOffering_DeveGerarId_QuandoForCriado()
    {
        // Arrange
        var serviceOffering = new ServiceOfferingBuilder()
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
        func.Should().Throw<ArgumentException>();
    }
}
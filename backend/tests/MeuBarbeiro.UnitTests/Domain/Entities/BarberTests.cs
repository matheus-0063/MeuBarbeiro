using FluentAssertions;
using MeuBarbeiro.Domain.Exceptions;
using MeuBarbeiro.UnitTests.TestBuilder;

namespace MeuBarbeiro.UnitTests.Domain.Entities;

public class BarberTests
{
    [Fact]
    public void Barber_DeveGerarId_QuandoForCriado()
    {
        // Act 
        var barber = new BarberBuilder()
            .Build();
        
        // Assert
        barber.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Barber_DeveFalhar_QuandoUserIdForVazio()
    {
        // Act
        var func = () => new BarberBuilder()
            .WithUserId(Guid.Empty)
            .Build();
        
        // Assert
        func.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AssignBarbershop_DeveAtribuirBarbershop_QuandoForValoresValidos()
    {
        // Arrange
        var barber = new BarberBuilder()
            .Build();

        var barbershop = new BarbershopBuilder()
            .WithOwnerUserId(Guid.NewGuid())
            .Build();
        
        // Act
        barber.AssignBarbershop(barbershop.Id);
        
        // Assert
        barber.BarbershopId.Should().Be(barbershop.Id);
    }

    [Fact]
    public void AssignBarbershop_DeveFalhar_QuandoBarbershopIdForVazio()
    {
        // Arrange
        var barber = new BarberBuilder()
            .Build();

        // Act
        var func = () => barber.AssignBarbershop(Guid.Empty);
        
        // Assert
        func.Should().Throw<ArgumentException>();
    }
    
    
    [Fact]
    public void AssignBarbershop_DeveFalhar_QuandoBarbershopIdJaTenhaValor()
    {
        // Arrange
        var barber = new BarberBuilder()
            .Build();

        var barbershop = new BarbershopBuilder()
            .WithOwnerUserId(Guid.NewGuid())
            .Build();
        
        // Act
        barber.AssignBarbershop(barbershop.Id);
        var func = () => barber.AssignBarbershop(Guid.NewGuid());
        
        // Assert
        func.Should().Throw<BarberBelongsAnotherBarbershopException>();
    }

    [Fact]
    public void RemoveBarbershop_DeveRemoverBarbershop_QuandoValoresValidos()
    {
        // Arrange
        var barber = new BarberBuilder()
            .Build();
        
        var barbershop = new BarbershopBuilder()
            .WithOwnerUserId(Guid.NewGuid())
            .Build();
        
        // Act
        barber.AssignBarbershop(barbershop.Id);
        barber.RemoveFromBarbershop(barbershop.Id);
        
        // Assert
        barber.BarbershopId.Should().BeNull();
    }
    
    [Fact]
    public void RemoveBarbershop_DeveFalhar_QuandoValoresValidos()
    {
        // Arrange
        var barber = new BarberBuilder()
            .Build();
        
        var barbershop = new BarbershopBuilder()
            .WithOwnerUserId(Guid.NewGuid())
            .Build();
        
        // Act
        barber.AssignBarbershop(barbershop.Id);
        var func = () => barber.RemoveFromBarbershop(Guid.Empty);
        
        // Assert
        func.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveBarbershop_DeveFalhar_QuandoBarbeiroPertencerAOutraBarbershop()
    {
        // Arrange
        var barber = new BarberBuilder()
            .Build();
        
        var barbershop = new BarbershopBuilder()
            .WithOwnerUserId(Guid.NewGuid())
            .Build();
        
        // Act
        barber.AssignBarbershop(Guid.NewGuid());
        var func = () => barber.RemoveFromBarbershop(barbershop.Id);
        
        // Assert
        func.Should().Throw<BarberDoesNotBelongBarbershopException>();
    }
}
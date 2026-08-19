using FluentAssertions;
using MeuBarbeiro.UnitTests.TestBuilder;

namespace MeuBarbeiro.UnitTests.Domain.Entities;

public class BarbershopTests
{
    [Fact]
    public void Barbershop_DeveGerarId_QuandoForCriado()
    {
        // Arrange 
        var barbershop = new BarbershopBuilder()
            .WithOwnerUserId(Guid.NewGuid())
            .Build();
        
        // Assert
        barbershop.Id.Should().NotBeEmpty();
        barbershop.OwnerUserId.Should().Be(barbershop.OwnerUserId);
    }
    
    [Fact]
    public void Barbershop_DeveFalhar_QuandoOwnerUserIdForEmpty()
    {
        // Arrange 
        var func = () => new BarbershopBuilder()
            .WithOwnerUserId(Guid.Empty)
            .Build();
        
        // Assert
        func.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Barbershop_DeveAtualizarDados_QuandoValoresForemAlterados()
    {
        // Arrange
        var barbershop = new BarbershopBuilder()
            .WithOwnerUserId(Guid.NewGuid())
            .Build();

        const string newName = "Barbearia Central";
        const string newCity = "Betim";
        const string newAddress = "Rua Padre Lage, 59";
        const string newDescription = "Lorem ipsum dolor sit amet";
        
        // Act
        barbershop.UpdateDetails(newName, newCity, newAddress, newDescription);
        
        // Assert
        barbershop.Name.Should().Be(newName);
        barbershop.City.Should().Be(newCity);
        barbershop.Address.Should().Be(newAddress);
        barbershop.Description.Should().Be(newDescription);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(5.1)]
    [InlineData(10)]
    public void MetodoUpdateAverageRating_DeveFalhar_QuandoValorForInvalido(double avaliacao)
    {
        // Arrange
        var barbershop = new BarbershopBuilder()
            .WithOwnerUserId(Guid.NewGuid())
            .Build();
        
        // Act 
        var act = () => barbershop.UpdateAverageRating(avaliacao);
        
        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2.5)]
    [InlineData(5)]
    public void MetodoUpdateAverageRating_DeveRetornarSucesso_QuandoValorForValido(double avaliacao)
    {
        // Arrange
        var barbershop = new BarbershopBuilder()
            .WithOwnerUserId(Guid.NewGuid())
            .Build();
        
        // Act
        barbershop.UpdateAverageRating(avaliacao);
        
        // Assert
        barbershop.AverageRating.Should().Be(avaliacao);
    }
    
    
}
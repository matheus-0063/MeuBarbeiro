using FluentAssertions;
using MeuBarbeiro.Domain.Enums;
using MeuBarbeiro.UnitTests.TestBuilder;

namespace MeuBarbeiro.UnitTests.Domain.Entities;

public class UserTests
{
    [Fact]
    public void User_DeveSerCriadoComRoleClient_QuandoForCliente()
    {
        // Act
        var user = new UserBuilder()
            .WithRole(UserRole.Client)
            .Build();

        // Assert
        user.Role.Should().Be(UserRole.Client);
    }

    [Fact]
    public void User_DeveSerCriadoComRoleBarbeiro_QuandoForBarbeiro()
    {
        // Act
        var user = new UserBuilder()
            .WithRole(UserRole.Barber)
            .Build();

        // Assert
        user.Role.Should().Be(UserRole.Barber);
    }

    [Fact]
    public void User_DeveSerCriadoComRoleBarbershopOwner_QuandoForBarbeshopOwner()
    {
        // Arrange
        var user = new UserBuilder()
            .WithRole(UserRole.BarbershopOwner)
            .Build();

        // Assert
        user.Role.Should().Be(UserRole.BarbershopOwner);
    }

    [Fact]
    public void User_DeveGerarId_QuandoForCriado()
    {
        // Act
        var user = new UserBuilder()
            .Build();

        // Assert
        user.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void User_DeveGuardarEmail_QuandoEmailForInformado()
    {
        // Arrange
        var email = "usuario@email.com";

        // Act
        var user = new UserBuilder()
            .WithEmail(email)
            .Build();

        // Assert
        user.Email.Should().Be(email);
    }

    [Fact]
    public void User_DeveGerarDataCriacao_QuandoForCriado()
    {
        // Act
        var user = new UserBuilder()
            .Build();

        // Assert
        user.CreateAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
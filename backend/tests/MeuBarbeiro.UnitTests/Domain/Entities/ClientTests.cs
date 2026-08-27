using FluentAssertions;
using MeuBarbeiro.UnitTests.TestBuilder;

namespace MeuBarbeiro.UnitTests.Domain.Entities;

public class ClientTests
{
    [Fact]
    public void Client_DeveGerarId_QuandoForCriado()
    {
        // Act
        var client = new ClientBuilder()
            .Build();

        // Assert
        client.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Client_DeveGuardarUserId_QuandoUserIdForInformado()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var client = new ClientBuilder()
            .WithUserId(userId)
            .Build();

        // Assert
        client.UserId.Should().Be(userId);
    }

    [Fact]
    public void Client_DeveFalhar_QuandoUserIdForVazio()
    {
        // Act
        var func = () => new ClientBuilder()
            .WithUserId(Guid.Empty)
            .Build();

        // Assert
        func.Should().Throw<ArgumentException>();
    }
}
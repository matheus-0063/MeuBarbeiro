using FluentAssertions;
using MeuBarbeiro.UnitTests.TestBuilder;

namespace MeuBarbeiro.UnitTests.Domain.Entities;

public class AppointmentTests
{
    [Fact]
    public void Appointment_DeveGerarId_QuandoForCriado()
    {
        // Act
        var appointment = new AppointmentBuilder()
            .Build();

        // Assert
        appointment.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Appointment_DeveFalhar_QuandoClientIdForVazio()
    {
        // Act 
        var appointment = () => new AppointmentBuilder()
            .WithClientId(Guid.Empty)
            .Build();
        
        // Assert
        appointment.Should().Throw<Exception>();
    }
}
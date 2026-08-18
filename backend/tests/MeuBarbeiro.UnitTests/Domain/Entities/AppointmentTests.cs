using FluentAssertions;
using MeuBarbeiro.Domain.Enums;
using MeuBarbeiro.Domain.Exceptions;
using MeuBarbeiro.UnitTests.TestBuilder;

namespace MeuBarbeiro.UnitTests.Domain.Entities;

public class AppointmentTests
{
    [Fact]
    public void Appointment_DeveGerarId_QuandoForCriado()
    {
        // Arrange
        var appointment = new AppointmentBuilder()
            .Build();

        // Assert
        appointment.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Appointment_DeveFalhar_QuandoClientIdForVazio()
    {
        // Arrange 
        var appointment = () => new AppointmentBuilder()
            .WithClientId(Guid.Empty)
            .Build();
        
        // Assert
        appointment.Should().Throw<ArgumentException>();
    }

    #region Accept Method
    [Fact]
    public void Accept_AlteraStatus_QuandoBarberIdEStatusSaoValidos()
    {
        // Arrange
        Guid barberId = Guid.NewGuid();
        
        var appointment = new AppointmentBuilder()
            .WithBarberId(barberId)
            .Build();
        
        // Act
        appointment.Accept(barberId);
        
        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Accepted);
        appointment.BarberId.Should().Be(barberId);
    }

    [Fact]
    public void Accept_DeveFalhar_QuandoBarberIdNaoForOMesmo()
    {
        // Arrange
        Guid barberId = Guid.NewGuid();
        
        var appointment = new AppointmentBuilder()
            .Build();
        
        // Act
        var act = () => appointment.Accept(barberId);
        
        // Assert
        act.Should().Throw<AppointmentActorNotAllowedException>();
        appointment.Status.Should().Be(AppointmentStatus.Pending);
    }
    
    [Fact]
    public void Accept_DeveFalhar_QuandoStatusNaoForPending()
    {
        // Arrange 
        Guid barberId = Guid.NewGuid();
        
        var appointment = new AppointmentBuilder()
            .WithBarberId(barberId)
            .Build();
        
        // Act
        appointment.Accept(barberId);
        var act = () => appointment.Accept(barberId);
        
        // Arrange
        act.Should().Throw<AppointmentStatusTransitionException>();
        appointment.Status.Should().Be(AppointmentStatus.Accepted);
    }
    #endregion
    
    #region Start Method

    [Fact]
    public void Start_AlteraStatusParaInProgress_QuandoBarberIdEStatusSaoValidos()
    {
        // Arrange
        var barberId = Guid.NewGuid();
        
        var appointment = new AppointmentBuilder()
            .WithBarberId(barberId)
            .Build();
        
        // Act 
        appointment.Accept(barberId);
        appointment.Start(barberId);
        
        // Assert
        appointment.Status.Should().Be(AppointmentStatus.InProgress);
    }

    [Fact]
    public void Start_DeveFalhar_QuandoBarberIdNaoForOMesmoDoAgendamento()
    {
        // Arrange
        var barberId = Guid.NewGuid();
        
        var appointment = new AppointmentBuilder()
            .WithBarberId(barberId)
            .Build();
        
        // Act
        appointment.Accept(barberId);
        var act = () => appointment.Start(Guid.NewGuid());
        
        // Assert
        act.Should().Throw<AppointmentActorNotAllowedException>();
        appointment.Status.Should().Be(AppointmentStatus.Accepted);
    }

    [Fact]
    public void Start_DeveFalhar_QuandoStatusNaoForAccepted()
    {
        // Arrange
        var barberId = Guid.NewGuid();
        
        var appointment = new AppointmentBuilder()
            .WithBarberId(barberId)
            .Build();
        
        // Act
        var act = () => appointment.Start(barberId);
        
        // Assert
        act.Should().Throw<InvalidOperationException>();
        appointment.Status.Should().Be(AppointmentStatus.Pending);
    }
    
    #endregion
    
    #region Complete Method

    [Fact]
    public void Complete_DeveAlterarStatusParaCompleted_QuandoBarberIdEStatusSaoValidos()
    {
        // Arrange
        var barberId = Guid.NewGuid();

        var appointment = new AppointmentBuilder()
            .WithBarberId(barberId)
            .Build();

        // Act
        appointment.Accept(barberId);
        appointment.Start(barberId);
        appointment.Complete(barberId);
        
        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public void Complete_DeveFalhar_QuandoBarberIdNaoForOMesmoDoAgendamento()
    {
        // Arrange 
        var barberId = Guid.NewGuid();
        var appointment = new AppointmentBuilder()
            .WithBarberId(barberId)
            .Build();
        
        // Act
        appointment.Accept(barberId);
        appointment.Start(barberId);
        var act = () => appointment.Complete(Guid.NewGuid());
        
        // Assert
        act.Should().Throw<AppointmentActorNotAllowedException>();
        appointment.Status.Should().Be(AppointmentStatus.InProgress);
    }

    [Fact]
    public void Complete_DeveFalhar_QuandoStatusNaoForInProgress()
    {
        // Arrange
        var barberId = Guid.NewGuid();
        
        var appointment = new AppointmentBuilder()
            .WithBarberId(barberId)
            .Build();
        
        // Act
        appointment.Accept(barberId);
        var act = () => appointment.Complete(barberId);
        
        // Assert 
        act.Should().Throw<InvalidOperationException>();
        appointment.Status.Should().Be(AppointmentStatus.Accepted);
    }
    
    #endregion
    
    #region Reject Method

    [Fact]
    public void Reject_DeveAlterarStatusParaRejected_QuandoBarberIdEStatusSaoValidos()
    {
        // Arrange
        var barberId = Guid.NewGuid();
        
        var appointment = new AppointmentBuilder()
            .WithBarberId(barberId)
            .Build();
        
        // Act
        appointment.Reject(barberId);
        
        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Rejected);
    }

    [Fact]
    public void Reject_DeveFalhar_QuandoBarberIdNaoForOMesmoDoAgendamento()
    {
        // Arrange
        var barberId = Guid.NewGuid();
        
        var appointment = new AppointmentBuilder()
            .WithBarberId(barberId)
            .Build();
        
        // Act
        var act = () => appointment.Reject(Guid.NewGuid());
        
        // Assert
        act.Should().Throw<AppointmentActorNotAllowedException>();
        appointment.Status.Should().Be(AppointmentStatus.Pending);
    }

    [Fact]
    public void Reject_DeveFalhar_QuandoStatusNaoForPending()
    {
        // Arrange
        var barberId = Guid.NewGuid();
        
        var appointment = new AppointmentBuilder()
            .WithBarberId(barberId)
            .Build();
        
        // Act
        appointment.Accept(barberId);
        var act = () => appointment.Reject(barberId);
        
        // Assert
        act.Should().Throw<InvalidOperationException>();
        appointment.Status.Should().Be(AppointmentStatus.Accepted);
    }
    
    #endregion
    
    #region Cancel Method

    [Fact]
    public void Cancel_DeveAlterarStatusParaCancel_QuandoBarberIdEStatusEHorarioSaoValidos()
    {
        // Arrange
        var barberId = Guid.NewGuid();
        var dateTime = DateTime.UtcNow;
        var utcNow = dateTime.AddHours(-3);
        
        var appointment = new AppointmentBuilder()
            .WithBarberId(barberId)
            .WithScheduledAtUtc(dateTime)
            .Build();
        
        // Act
        appointment.Accept(barberId);
        appointment.Cancel(barberId, utcNow);
        
        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }
    
    [Fact]
    public void Cancel_DeveAlterarStatusParaCancel_QuandoClientIdEStatusEHorarioSaoValidos()
    {
        // Arrange
        var barberId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var dateTime = DateTime.UtcNow;
        var utcNow = dateTime.AddHours(-3);
        
        var appointment = new AppointmentBuilder()
            .WithBarberId(barberId)
            .WithClientId(clientId)
            .WithScheduledAtUtc(dateTime)
            .Build();
        
        // Act
        appointment.Accept(barberId);
        appointment.Cancel(clientId, utcNow);
        
        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public void Cancel_DeveFalhar_QuandoUserIdNaoForOMesmoDoClientIdOuBarberIdDoAgendamento()
    {
        // Arrange
        var barberId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var dateTime = DateTime.UtcNow;
        var utcNow = dateTime.AddHours(-3);
        
        var appointment = new AppointmentBuilder()
            .WithBarberId(barberId)
            .WithClientId(clientId)
            .WithScheduledAtUtc(dateTime)
            .Build();
        
        // Act
        appointment.Accept(barberId);
        var act = () => appointment.Cancel(Guid.NewGuid(), utcNow);
        
        // Assert
        act.Should().Throw<ArgumentException>();
        appointment.Status.Should().Be(AppointmentStatus.Accepted);
    }

    [Fact]
    public void Cancel_DeveFalhar_QuandoHorarioDeDesistenciaForInvalido()
    {
        // Arrange
        var barberId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var dateTime = DateTime.UtcNow;
        var utcNow = dateTime.AddHours(-1);
        
        var appointment = new AppointmentBuilder()
            .WithBarberId(barberId)
            .WithClientId(clientId)
            .WithScheduledAtUtc(dateTime)
            .Build();
        
        // Act
        appointment.Accept(barberId);
        var act = () => appointment.Cancel(clientId, utcNow);
        
        // Assert
        act.Should().Throw<InvalidOperationException>();
        appointment.Status.Should().Be(AppointmentStatus.Accepted);
    }

    [Fact]
    public void Cancel_DeveFalhar_QuandoStatusForInvalido()
    {
        // Arrange
        var barberId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var dateTime = DateTime.UtcNow;
        var utcNow = dateTime.AddHours(-3);
        
        var appointment = new AppointmentBuilder()
            .WithBarberId(barberId)
            .WithClientId(clientId)
            .WithScheduledAtUtc(dateTime)
            .Build();
        
        // Act
        appointment.Accept(barberId);
        appointment.Start(barberId);
        appointment.Complete(barberId);
        var act = () => appointment.Cancel(clientId, utcNow);
        
        // Assert
        act.Should().Throw<InvalidOperationException>();
        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }
    #endregion
}
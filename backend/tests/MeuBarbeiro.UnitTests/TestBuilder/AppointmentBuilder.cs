using MeuBarbeiro.Domain.Entities;
using MeuBarbeiro.Domain.Enums;

namespace MeuBarbeiro.UnitTests.TestBuilder;

public class AppointmentBuilder
{
    private Guid _clientId = Guid.NewGuid();
    private Guid _barberId = Guid.NewGuid();
    private Guid _barbershopId = Guid.NewGuid();
    private Guid _serviceId = Guid.NewGuid();
    private DateTime _scheduledAt = DateTime.Today.AddDays(1);
    private decimal _totalPrice = 0;
    private AppointmentStatus _status = AppointmentStatus.Pending;

    public AppointmentBuilder WithClientId(Guid clientId)
    {
        _clientId = clientId;
        return this;
    }

    public AppointmentBuilder WithBarberId(Guid barberId)
    {
        _barberId = barberId;
        return this;
    }

    public AppointmentBuilder WithBarbershopId(Guid barbershopId)
    {
        _barbershopId = barbershopId;
        return this;
    }

    public AppointmentBuilder WithServiceId(Guid serviceId)
    {
        _serviceId = serviceId;
        return this;
    }

    public AppointmentBuilder WithTotalPrice(decimal totalPrice)
    {
        _totalPrice = totalPrice;
        return this;
    }

    public AppointmentBuilder WithStatus(AppointmentStatus newStatus)
    {
        _status = newStatus;
        return this;
    }

    public Appointment Build()
    {
        var appointment = new Appointment(
            id: Guid.NewGuid(),    
            clientId: _clientId,
            barberId: _barberId,
            barbershopId: _barbershopId,
            scheduledAtUtc: _scheduledAt,
            totalPrice: _totalPrice,
            status: _status);

        return appointment;
    }
}
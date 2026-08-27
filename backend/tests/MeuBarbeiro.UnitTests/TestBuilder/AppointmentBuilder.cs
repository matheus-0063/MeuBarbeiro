using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.UnitTests.TestBuilder;

public class AppointmentBuilder
{
    private Guid _barberId = Guid.NewGuid();
    private Guid _barbershopId = Guid.NewGuid();
    private Guid _clientId = Guid.NewGuid();
    private DateTime _scheduledAt = DateTime.UtcNow.AddDays(1);
    private Guid _serviceId = Guid.NewGuid();
    private decimal _totalPrice = 40.0m;

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

    public AppointmentBuilder WithScheduledAtUtc(DateTime scheduledAt)
    {
        _scheduledAt = scheduledAt;
        return this;
    }

    public AppointmentBuilder WithTotalPrice(decimal totalPrice)
    {
        _totalPrice = totalPrice;
        return this;
    }

    public Appointment Build()
    {
        var appointment = new Appointment(
            _clientId,
            _barberId,
            _barbershopId,
            _scheduledAt,
            _totalPrice);

        return appointment;
    }
}
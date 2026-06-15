using FluentValidation.Results;
using MeuBarbeiro.Application.Abstractions.Messaging;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.DTOs.Appointments;
using MeuBarbeiro.Application.Services;
using MeuBarbeiro.Contracts.Events;
using MeuBarbeiro.Domain.Entities;
using MeuBarbeiro.Domain.Enums;

namespace MeuBarbeiro.Api.Tests;

public class AppointmentServiceTests
{
    [Fact]
    public async Task CreateAppointment_ShouldReturnSuccessAndPendingStatus_WhenRepositoryAccepts()
    {
        var repository = new FakeAppointmentRepository();
        var request = BuildCreateRequest();
        var barberRepository = new FakeBarberRepository
        {
            BarberByBarbershopId = BuildBarber(barbershopId: request.BarbershopId)
        };
        var publisher = new FakeEventPublisher();
        var service = new AppointmentService(repository, barberRepository, publisher);

        var clientId = Guid.NewGuid();
        var result = await service.CreateAppointment(request, clientId);

        Assert.True(result.IsValid);
        Assert.NotEqual(Guid.Empty, result.Data);
        Assert.NotNull(repository.LastAddedAppointment);
        Assert.Equal(AppointmentStatus.Pending, repository.LastAddedAppointment!.Status);
        Assert.Equal(clientId, repository.LastAddedAppointment!.ClientId);
        Assert.Equal(request.BarbershopId, repository.LastAddedAppointment!.BarbershopId);
        Assert.Equal(barberRepository.BarberByBarbershopId!.Id, repository.LastAddedAppointment!.BarberId);
        Assert.Single(publisher.PublishedMessages);
        Assert.IsType<AppointmentRequestedIntegrationEvent>(publisher.PublishedMessages.Single());
    }

    [Fact]
    public async Task CreateAppointment_ShouldReturnFailure_WhenRepositoryRejects()
    {
        var request = BuildCreateRequest();
        var repository = new FakeAppointmentRepository
        {
            AddResult = BuildInvalidResult("Falha ao criar agendamento.")
        };
        var barberRepository = new FakeBarberRepository
        {
            BarberByBarbershopId = BuildBarber(barbershopId: request.BarbershopId)
        };
        var publisher = new FakeEventPublisher();
        var service = new AppointmentService(repository, barberRepository, publisher);

        var result = await service.CreateAppointment(request, Guid.NewGuid());

        Assert.False(result.IsValid);
        Assert.Single(result.ValidationResult.Errors);
        Assert.Empty(publisher.PublishedMessages);
    }

    [Fact]
    public async Task CreateAppointment_ShouldReturnFailure_WhenBarbershopDoesNotHaveBarber()
    {
        var request = BuildCreateRequest();
        var repository = new FakeAppointmentRepository();
        var barberRepository = new FakeBarberRepository();
        var publisher = new FakeEventPublisher();
        var service = new AppointmentService(repository, barberRepository, publisher);

        var result = await service.CreateAppointment(request, Guid.NewGuid());

        Assert.False(result.IsValid);
        Assert.Contains(result.ValidationResult.Errors, error => error.PropertyName == nameof(CreateAppointmentRequestDto.BarbershopId));
        Assert.Null(repository.LastAddedAppointment);
        Assert.Empty(publisher.PublishedMessages);
    }

    [Fact]
    public async Task GetAppointment_ShouldReturnSuccessWithDto_WhenAppointmentExists()
    {
        var appointment = BuildAppointment();
        var repository = new FakeAppointmentRepository
        {
            AppointmentById = appointment
        };
        var service = new AppointmentService(repository, new FakeBarberRepository(), new FakeEventPublisher());

        var result = await service.GetAppointment(appointment.Id);

        Assert.True(result.IsValid);
        Assert.False(result.IsNotFound);
        Assert.NotNull(result.Data);
        Assert.Equal(appointment.Id, result.Data!.Id);
        Assert.Equal(appointment.Status.ToString(), result.Data.Status);
    }

    [Fact]
    public async Task GetAppointment_ShouldReturnNotFound_WhenAppointmentDoesNotExist()
    {
        var service = new AppointmentService(new FakeAppointmentRepository(), new FakeBarberRepository(), new FakeEventPublisher());

        var result = await service.GetAppointment(Guid.NewGuid());

        Assert.True(result.IsNotFound);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetListAppointments_ShouldListClientAppointments_WhenUserTypeIsClient()
    {
        var clientId = Guid.NewGuid();
        var repository = new FakeAppointmentRepository
        {
            ClientAppointments =
            [
                BuildAppointment(clientId: clientId, status: AppointmentStatus.Pending),
                BuildAppointment(clientId: clientId, status: AppointmentStatus.Accepted)
            ]
        };
        var service = new AppointmentService(repository, new FakeBarberRepository(), new FakeEventPublisher());

        var result = await service.GetListAppointments(clientId, AppointmentUserType.Client);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data!.Count());
    }

    [Fact]
    public async Task GetListAppointments_ShouldListBarberAppointments_WhenUserTypeIsBarber()
    {
        var barberId = Guid.NewGuid();
        var repository = new FakeAppointmentRepository
        {
            BarberAppointments =
            [
                BuildAppointment(barberId: barberId),
                BuildAppointment(barberId: barberId)
            ]
        };
        var service = new AppointmentService(repository, new FakeBarberRepository(), new FakeEventPublisher());

        var result = await service.GetListAppointments(barberId, AppointmentUserType.Barber);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data!.Count());
    }

    [Fact]
    public async Task GetListAppointments_ShouldReturnFailure_WhenUserTypeIsInvalid()
    {
        var service = new AppointmentService(new FakeAppointmentRepository(), new FakeBarberRepository(), new FakeEventPublisher());

        var result = await service.GetListAppointments(Guid.NewGuid(), (AppointmentUserType)999);

        Assert.False(result.IsValid);
        Assert.Contains(result.ValidationResult.Errors, error => error.PropertyName == "userType");
    }

    [Fact]
    public async Task GetListAppointments_ShouldFilterByStatus_WhenStatusIsProvided()
    {
        var clientId = Guid.NewGuid();
        var repository = new FakeAppointmentRepository
        {
            ClientAppointments =
            [
                BuildAppointment(clientId: clientId, status: AppointmentStatus.Pending),
                BuildAppointment(clientId: clientId, status: AppointmentStatus.Accepted),
                BuildAppointment(clientId: clientId, status: AppointmentStatus.Pending)
            ]
        };
        var service = new AppointmentService(repository, new FakeBarberRepository(), new FakeEventPublisher());

        var result = await service.GetListAppointments(clientId, AppointmentUserType.Client, AppointmentStatus.Pending);

        Assert.True(result.IsValid);
        Assert.Equal(2, result.Data!.Count());
        Assert.All(result.Data!, dto => Assert.Equal(nameof(AppointmentStatus.Pending), dto.Status));
    }

    [Fact]
    public async Task GetListAppointments_ShouldNotFilterByStatus_WhenStatusIsNotProvided()
    {
        var clientId = Guid.NewGuid();
        var repository = new FakeAppointmentRepository
        {
            ClientAppointments =
            [
                BuildAppointment(clientId: clientId, status: AppointmentStatus.Pending),
                BuildAppointment(clientId: clientId, status: AppointmentStatus.Accepted)
            ]
        };
        var service = new AppointmentService(repository, new FakeBarberRepository(), new FakeEventPublisher());

        var result = await service.GetListAppointments(clientId, AppointmentUserType.Client, null);

        Assert.True(result.IsValid);
        Assert.Equal(2, result.Data!.Count());
    }

    [Fact]
    public async Task UpdateStatusAppointment_ShouldReturnSuccessAndChangeStatus_WhenAppointmentExists()
    {
        var appointment = BuildAppointment(status: AppointmentStatus.Pending);
        var repository = new FakeAppointmentRepository
        {
            AppointmentById = appointment
        };
        var publisher = new FakeEventPublisher();
        var service = new AppointmentService(repository, new FakeBarberRepository(), publisher);

        var result = await service.UpdateStatusAppointment(new UpdateAppointmentStatusRequestDto
        {
            AppointmentId = appointment.Id,
            Status = AppointmentStatus.Accepted
        });

        Assert.True(result.IsValid);
        Assert.False(result.IsNotFound);
        Assert.Equal(AppointmentStatus.Accepted, appointment.Status);
        Assert.NotNull(repository.LastUpdatedAppointment);
        Assert.Equal(AppointmentStatus.Accepted, repository.LastUpdatedAppointment!.Status);
        Assert.Single(publisher.PublishedMessages);
        Assert.IsType<AppointmentStatusUpdatedIntegrationEvent>(publisher.PublishedMessages.Single());
    }

    [Fact]
    public async Task UpdateStatusAppointment_ShouldReturnNotFound_WhenAppointmentDoesNotExist()
    {
        var publisher = new FakeEventPublisher();
        var service = new AppointmentService(new FakeAppointmentRepository(), new FakeBarberRepository(), publisher);

        var result = await service.UpdateStatusAppointment(new UpdateAppointmentStatusRequestDto
        {
            AppointmentId = Guid.NewGuid(),
            Status = AppointmentStatus.Completed
        });

        Assert.True(result.IsNotFound);
        Assert.False(result.Data);
        Assert.Empty(publisher.PublishedMessages);
    }

    [Fact]
    public async Task UpdateStatusAppointment_ShouldPropagateFailure_WhenRepositoryRejectsUpdate()
    {
        var appointment = BuildAppointment();
        var repository = new FakeAppointmentRepository
        {
            AppointmentById = appointment,
            UpdateResult = BuildInvalidResult("Falha ao atualizar.")
        };
        var publisher = new FakeEventPublisher();
        var service = new AppointmentService(repository, new FakeBarberRepository(), publisher);

        var result = await service.UpdateStatusAppointment(new UpdateAppointmentStatusRequestDto
        {
            AppointmentId = appointment.Id,
            Status = AppointmentStatus.Cancelled
        });

        Assert.False(result.IsValid);
        Assert.Single(result.ValidationResult.Errors);
        Assert.Empty(publisher.PublishedMessages);
    }

    private static CreateAppointmentRequestDto BuildCreateRequest()
    {
        return new CreateAppointmentRequestDto
        {
            BarbershopId = Guid.NewGuid(),
            ScheduledAtUtc = DateTime.UtcNow.AddDays(1),
            TotalPrice = 50m
        };
    }

    private static Appointment BuildAppointment(
        Guid? id = null,
        Guid? clientId = null,
        Guid? barberId = null,
        Guid? barbershopId = null,
        AppointmentStatus status = AppointmentStatus.Pending)
    {
        return new Appointment
        {
            Id = id ?? Guid.NewGuid(),
            ClientId = clientId ?? Guid.NewGuid(),
            BarberId = barberId ?? Guid.NewGuid(),
            BarbershopId = barbershopId ?? Guid.NewGuid(),
            ScheduledAtUtc = DateTime.UtcNow.AddHours(5),
            TotalPrice = 75m,
            Status = status
        };
    }

    private static Barber BuildBarber(Guid? userId = null, Guid? barbershopId = null)
    {
        return new Barber(userId ?? Guid.NewGuid(), barbershopId ?? Guid.NewGuid());
    }

    private static ValidationResult BuildInvalidResult(string errorMessage)
    {
        return new ValidationResult(
        [
            new ValidationFailure("ErrorMessages", errorMessage)
        ]);
    }

    private sealed class FakeAppointmentRepository : IAppointmentRepository
    {
        public IReadOnlyCollection<Appointment> ClientAppointments { get; set; } = Array.Empty<Appointment>();
        public IReadOnlyCollection<Appointment> BarberAppointments { get; set; } = Array.Empty<Appointment>();
        public Appointment? AppointmentById { get; set; }
        public Appointment? LastAddedAppointment { get; private set; }
        public Appointment? LastUpdatedAppointment { get; private set; }
        public ValidationResult AddResult { get; set; } = new();
        public ValidationResult UpdateResult { get; set; } = new();

        public Task<IReadOnlyCollection<Appointment>> ListByBarberAsync(Guid barberId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(BarberAppointments);
        }

        public Task<IReadOnlyCollection<Appointment>> ListByClientAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ClientAppointments);
        }

        public Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(AppointmentById);
        }

        public Task<ValidationResult> AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
        {
            LastAddedAppointment = appointment;
            AppointmentById = appointment;
            return Task.FromResult(AddResult);
        }

        public Task<ValidationResult> UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default)
        {
            LastUpdatedAppointment = appointment;
            AppointmentById = appointment;
            return Task.FromResult(UpdateResult);
        }
    }

    private sealed class FakeEventPublisher : IEventPublisher
    {
        public List<object> PublishedMessages { get; } = [];

        public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        {
            PublishedMessages.Add(message!);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeBarberRepository : IBarberRepository
    {
        public Barber? BarberById { get; set; }
        public Barber? BarberByBarbershopId { get; set; }

        public Task<Barber?> GetByIdAsync(Guid barberId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(BarberById);
        }

        public Task<Barber?> GetByBarbershopIdAsync(Guid barbershopId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(BarberByBarbershopId);
        }

        public Task<Barber?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Barber?>(null);
        }

        public Task<IReadOnlyCollection<Barber>> ListByBarbershopAsync(Guid barbershopId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Barber>>(Array.Empty<Barber>());
        }

        public Task<ValidationResult> AddAsync(Barber barber, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ValidationResult());
        }

        public Task<ValidationResult> UpdateAsync(Barber barber, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}

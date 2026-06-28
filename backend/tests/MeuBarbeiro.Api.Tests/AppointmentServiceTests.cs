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
        var selectionRepository = new FakeAppointmentServiceSelectionRepository();
        var request = BuildCreateRequest();
        var barberRepository = new FakeBarberRepository
        {
            BarberByBarbershopId = BuildBarber(barbershopId: request.BarbershopId)
        };
        var serviceOfferingRepository = new FakeServiceOfferingRepository
        {
            ServicesByIds = request.ServiceIds.Select(serviceId => BuildServiceOffering(serviceId, request.BarbershopId)).ToArray()
        };
        var publisher = new FakeEventPublisher();
        var service = new AppointmentService(
            repository,
            selectionRepository,
            barberRepository,
            new FakeBarbershopRepository(),
            new FakeClientRepository(),
            serviceOfferingRepository,
            new FakeUserRepository(),
            publisher);

        var clientId = Guid.NewGuid();
        var result = await service.CreateAppointment(request, clientId);

        Assert.True(result.IsValid);
        Assert.NotEqual(Guid.Empty, result.Data);
        Assert.NotNull(repository.LastAddedAppointment);
        Assert.Equal(AppointmentStatus.Pending, repository.LastAddedAppointment!.Status);
        Assert.Equal(clientId, repository.LastAddedAppointment!.ClientId);
        Assert.Equal(request.BarbershopId, repository.LastAddedAppointment!.BarbershopId);
        Assert.Equal(barberRepository.BarberByBarbershopId!.Id, repository.LastAddedAppointment!.BarberId);
        Assert.Equal(request.ServiceIds.Count, selectionRepository.LastAddedSelections.Count);
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
        var selectionRepository = new FakeAppointmentServiceSelectionRepository();
        var barberRepository = new FakeBarberRepository
        {
            BarberByBarbershopId = BuildBarber(barbershopId: request.BarbershopId)
        };
        var serviceOfferingRepository = new FakeServiceOfferingRepository
        {
            ServicesByIds = request.ServiceIds.Select(serviceId => BuildServiceOffering(serviceId, request.BarbershopId)).ToArray()
        };
        var publisher = new FakeEventPublisher();
        var service = new AppointmentService(
            repository,
            selectionRepository,
            barberRepository,
            new FakeBarbershopRepository(),
            new FakeClientRepository(),
            serviceOfferingRepository,
            new FakeUserRepository(),
            publisher);

        var result = await service.CreateAppointment(request, Guid.NewGuid());

        Assert.False(result.IsValid);
        Assert.Single(result.ValidationResult.Errors);
        Assert.Empty(selectionRepository.LastAddedSelections);
        Assert.Empty(publisher.PublishedMessages);
    }

    [Fact]
    public async Task CreateAppointment_ShouldReturnFailure_WhenBarbershopDoesNotHaveBarber()
    {
        var request = BuildCreateRequest();
        var repository = new FakeAppointmentRepository();
        var selectionRepository = new FakeAppointmentServiceSelectionRepository();
        var barberRepository = new FakeBarberRepository();
        var publisher = new FakeEventPublisher();
        var service = new AppointmentService(
            repository,
            selectionRepository,
            barberRepository,
            new FakeBarbershopRepository(),
            new FakeClientRepository(),
            new FakeServiceOfferingRepository(),
            new FakeUserRepository(),
            publisher);

        var result = await service.CreateAppointment(request, Guid.NewGuid());

        Assert.False(result.IsValid);
        Assert.Contains(result.ValidationResult.Errors, error => error.PropertyName == nameof(CreateAppointmentRequestDto.BarbershopId));
        Assert.Null(repository.LastAddedAppointment);
        Assert.Empty(selectionRepository.LastAddedSelections);
        Assert.Empty(publisher.PublishedMessages);
    }

    [Fact]
    public async Task GetAppointment_ShouldReturnSuccessWithDto_WhenAppointmentExists()
    {
        var appointment = BuildAppointment();
        var user = BuildUser(name: "Cliente Teste");
        var client = BuildClient(clientId: appointment.ClientId, userId: user.Id);
        var repository = new FakeAppointmentRepository
        {
            AppointmentById = appointment
        };
        var service = BuildService(
            repository: repository,
            client: (client, user),
            barbershops: [BuildBarbershop(id: appointment.BarbershopId, name: "Barbearia A")]);

        var result = await service.GetAppointment(appointment.Id);

        Assert.True(result.IsValid);
        Assert.False(result.IsNotFound);
        Assert.NotNull(result.Data);
        Assert.Equal(appointment.Id, result.Data!.Id);
        Assert.Equal(appointment.Status.ToString(), result.Data.Status);
        Assert.Equal("Cliente Teste", result.Data.ClientName);
        Assert.Equal("Barbearia A", result.Data.BarbershopName);
    }

    [Fact]
    public async Task GetAppointment_ShouldReturnNotFound_WhenAppointmentDoesNotExist()
    {
        var service = BuildService();

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
        var service = BuildService(repository: repository, clients: [BuildClient(clientId: clientId, userId: Guid.Parse("11111111-1111-1111-1111-111111111111"))], users: [BuildUser(id: Guid.Parse("11111111-1111-1111-1111-111111111111"), name: "Cliente 1")], barbershops: [BuildBarbershop()]);

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
        var service = BuildService(repository: repository, barbershops: [BuildBarbershop()]);

        var result = await service.GetListAppointments(barberId, AppointmentUserType.Barber);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data!.Count());
    }

    [Fact]
    public async Task GetListAppointments_ShouldReturnFailure_WhenUserTypeIsInvalid()
    {
        var service = BuildService();

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
        var service = BuildService(repository: repository, clients: [BuildClient(clientId: clientId, userId: Guid.Parse("22222222-2222-2222-2222-222222222222"))], users: [BuildUser(id: Guid.Parse("22222222-2222-2222-2222-222222222222"), name: "Cliente 2")], barbershops: [BuildBarbershop()]);

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
        var service = BuildService(repository: repository, clients: [BuildClient(clientId: clientId, userId: Guid.Parse("33333333-3333-3333-3333-333333333333"))], users: [BuildUser(id: Guid.Parse("33333333-3333-3333-3333-333333333333"), name: "Cliente 3")], barbershops: [BuildBarbershop()]);

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
        var service = BuildService(repository: repository, publisher: publisher);

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
        var service = BuildService(publisher: publisher);

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
        var service = BuildService(repository: repository, publisher: publisher);

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
            ServiceIds = [Guid.NewGuid(), Guid.NewGuid()],
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

    private static Client BuildClient(Guid? clientId = null, Guid? userId = null)
    {
        var client = new Client(userId ?? Guid.NewGuid());

        if (clientId.HasValue)
        {
            typeof(Client).GetProperty(nameof(Client.Id))!.SetValue(client, clientId.Value);
        }

        return client;
    }

    private static (Client client, User user) BuildClientWithUser()
    {
        var user = BuildUser(name: "Cliente");
        var client = BuildClient(userId: user.Id);
        return (client, user);
    }

    private static User BuildUser(Guid? id = null, string name = "Cliente", string email = "cliente@email.com")
    {
        var user = new User(name, email, "hash", UserRole.Client);

        if (id.HasValue)
        {
            typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, id.Value);
        }

        return user;
    }

    private static Barbershop BuildBarbershop(Guid? id = null, string name = "Barbearia")
    {
        var barbershop = new Barbershop
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            City = "Cidade",
            Address = "Rua",
            Description = "Descricao"
        };

        return barbershop;
    }

    private static ServiceOffering BuildServiceOffering(Guid serviceId, Guid barbershopId)
    {
        return new ServiceOffering
        {
            Id = serviceId,
            BarbershopId = barbershopId,
            Name = $"Servico {serviceId.ToString()[..4]}",
            Price = 25m,
            Description = "Servico",
            DurationMinutes = 30
        };
    }

    private AppointmentService BuildService(
        FakeAppointmentRepository? repository = null,
        FakeAppointmentServiceSelectionRepository? selectionRepository = null,
        FakeBarberRepository? barberRepository = null,
        FakeBarbershopRepository? barbershopRepository = null,
        FakeClientRepository? clientRepository = null,
        FakeServiceOfferingRepository? serviceOfferingRepository = null,
        FakeUserRepository? userRepository = null,
        FakeEventPublisher? publisher = null,
        (Client client, User user)? client = null,
        IReadOnlyCollection<Client>? clients = null,
        IReadOnlyCollection<User>? users = null,
        IReadOnlyCollection<Barbershop>? barbershops = null)
    {
        var fakeClientRepository = clientRepository ?? new FakeClientRepository();
        var fakeUserRepository = userRepository ?? new FakeUserRepository();
        var fakeBarbershopRepository = barbershopRepository ?? new FakeBarbershopRepository();

        if (client.HasValue)
        {
            fakeClientRepository.ClientsByIds = [client.Value.client];
            fakeUserRepository.UsersByIds = [client.Value.user];
        }

        if (clients is not null)
        {
            fakeClientRepository.ClientsByIds = clients;
        }

        if (users is not null)
        {
            fakeUserRepository.UsersByIds = users;
        }

        if (barbershops is not null)
        {
            fakeBarbershopRepository.BarbershopsByIds = barbershops;
        }

        return new AppointmentService(
            repository ?? new FakeAppointmentRepository(),
            selectionRepository ?? new FakeAppointmentServiceSelectionRepository(),
            barberRepository ?? new FakeBarberRepository(),
            fakeBarbershopRepository,
            fakeClientRepository,
            serviceOfferingRepository ?? new FakeServiceOfferingRepository(),
            fakeUserRepository,
            publisher ?? new FakeEventPublisher());
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

    private sealed class FakeAppointmentServiceSelectionRepository : IAppointmentServiceSelectionRepository
    {
        public List<AppointmentServiceSelection> LastAddedSelections { get; } = [];
        public IReadOnlyCollection<AppointmentServiceSelection> SelectionsByAppointmentIds { get; set; } = Array.Empty<AppointmentServiceSelection>();

        public Task<ValidationResult> AddRangeAsync(IEnumerable<AppointmentServiceSelection> selections, CancellationToken cancellationToken = default)
        {
            LastAddedSelections.AddRange(selections);
            return Task.FromResult(new ValidationResult());
        }

        public Task<IReadOnlyCollection<AppointmentServiceSelection>> ListByAppointmentIdsAsync(IEnumerable<Guid> appointmentIds, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SelectionsByAppointmentIds);
        }
    }

    private sealed class FakeBarbershopRepository : IBarbershopRepository
    {
        public IReadOnlyCollection<Barbershop> BarbershopsByIds { get; set; } = Array.Empty<Barbershop>();

        public Task<ValidationResult> AddAsync(Barbershop barbershop, CancellationToken cancellationToken = default) => Task.FromResult(new ValidationResult());
        public Task<Barbershop?> GetByIdAsync(Guid barbershopId, CancellationToken cancellationToken = default) => Task.FromResult(BarbershopsByIds.FirstOrDefault(x => x.Id == barbershopId));
        public Task<IReadOnlyCollection<Barbershop>> ListByIdsAsync(IEnumerable<Guid> barbershopIds, CancellationToken cancellationToken = default) => Task.FromResult(BarbershopsByIds);
        public Task<IReadOnlyCollection<Barbershop>> ListAsync(string? city = null, CancellationToken cancellationToken = default) => Task.FromResult(BarbershopsByIds);
        public Task<ValidationResult> UpdateAsync(Barbershop barbershop, CancellationToken cancellationToken = default) => Task.FromResult(new ValidationResult());
    }

    private sealed class FakeClientRepository : IClientRepository
    {
        public IReadOnlyCollection<Client> ClientsByIds { get; set; } = Array.Empty<Client>();

        public Task<Client?> GetByIdAsync(Guid clientId, CancellationToken cancellationToken = default) => Task.FromResult(ClientsByIds.FirstOrDefault(x => x.Id == clientId));
        public Task<Client?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(ClientsByIds.FirstOrDefault(x => x.UserId == userId));
        public Task<IReadOnlyCollection<Client>> ListByIdsAsync(IEnumerable<Guid> clientIds, CancellationToken cancellationToken = default) => Task.FromResult(ClientsByIds);
        public Task<ValidationResult> AddAsync(Client client, CancellationToken cancellationToken = default) => Task.FromResult(new ValidationResult());
    }

    private sealed class FakeServiceOfferingRepository : IServiceOfferingRepository
    {
        public IReadOnlyCollection<ServiceOffering> ServicesByIds { get; set; } = Array.Empty<ServiceOffering>();

        public Task<ValidationResult> AddAsync(ServiceOffering serviceOffering, CancellationToken cancellationToken = default) => Task.FromResult(new ValidationResult());
        public Task<IReadOnlyCollection<ServiceOffering>> ListByIdsAsync(IEnumerable<Guid> serviceOfferingIds, CancellationToken cancellationToken = default) => Task.FromResult(ServicesByIds);
        public Task<IReadOnlyCollection<ServiceOffering>> ListByBarbershopAsync(Guid barbershopId, CancellationToken cancellationToken = default) => Task.FromResult(ServicesByIds.Where(x => x.BarbershopId == barbershopId).ToArray() as IReadOnlyCollection<ServiceOffering>);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public IReadOnlyCollection<User> UsersByIds { get; set; } = Array.Empty<User>();

        public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(UsersByIds.FirstOrDefault(x => x.Id == userId));
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(UsersByIds.FirstOrDefault(x => x.Email == email));
        public Task<IReadOnlyCollection<User>> ListByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default) => Task.FromResult(UsersByIds);
        public Task<ValidationResult> AddAsync(User user, CancellationToken cancellationToken = default) => Task.FromResult(new ValidationResult());
    }
}

using FluentAssertions;
using FluentValidation.Results;
using MeuBarbeiro.Application.Abstractions.Messaging;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.DTOs.Appointments;
using MeuBarbeiro.Application.Services;
using MeuBarbeiro.Contracts.Events;
using MeuBarbeiro.Domain.Entities;
using MeuBarbeiro.Domain.Enums;
using MeuBarbeiro.UnitTests.TestBuilder;
using Moq;

namespace MeuBarbeiro.UnitTests.Application.Services;

public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _mockAppointmentRepository;
    private readonly Mock<IAppointmentServiceSelectionRepository> _mockSelectionRepository;
    private readonly Mock<IBarberRepository> _mockBarberRepository;
    private readonly Mock<IBarbershopRepository> _mockBarbershopRepository;
    private readonly Mock<IClientRepository> _mockClientRepository;
    private readonly Mock<IReviewRepository> _mockReviewRepository;
    private readonly Mock<IServiceOfferingRepository> _mockServiceOfferingRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IEventPublisher> _mockEventPublisher;

    private readonly AppointmentService _service;

    public AppointmentServiceTests()
    {
        _mockAppointmentRepository = new Mock<IAppointmentRepository>();
        _mockSelectionRepository = new Mock<IAppointmentServiceSelectionRepository>();
        _mockBarberRepository = new Mock<IBarberRepository>();
        _mockBarbershopRepository = new Mock<IBarbershopRepository>();
        _mockClientRepository = new Mock<IClientRepository>();
        _mockReviewRepository = new Mock<IReviewRepository>();
        _mockServiceOfferingRepository = new Mock<IServiceOfferingRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockEventPublisher = new Mock<IEventPublisher>();

        _service = new AppointmentService(_mockAppointmentRepository.Object, _mockSelectionRepository.Object,
            _mockBarberRepository.Object, _mockBarbershopRepository.Object,
            _mockClientRepository.Object, _mockReviewRepository.Object, _mockServiceOfferingRepository.Object,
            _mockUserRepository.Object, _mockEventPublisher.Object);
    }

    [Fact]
    public async Task CreateAppointment_DeveCriarUmAgendamento_QuandoRequestForValido()
    {
        // Arrange 

        #region Builders

        var client = new ClientBuilder()
            .Build();

        var barbershop = new BarbershopBuilder()
            .WithName("Barbershop")
            .WithAddress("Rua Padre Lage, 59")
            .WithCity("Betim")
            .Build();

        var barber = new BarberBuilder()
            .WithBarbershopId(barbershop.Id)
            .WithUserId(Guid.NewGuid())
            .Build();

        var corte = new ServiceOfferingBuilder()
            .WithName("Corte")
            .WithDescription("Corte na preferencia do cliente")
            .WithPrice(50.0m)
            .WithDurationMinutes(30)
            .WithBarbershopId(barbershop.Id)
            .Build();

        var barba = new ServiceOfferingBuilder()
            .WithName("Barba")
            .WithDescription("Barba na preferencia do cliente")
            .WithPrice(40.0m)
            .WithDurationMinutes(20)
            .WithBarbershopId(barbershop.Id)
            .Build();

        #endregion

        var listServiceOffering = new List<ServiceOffering> { corte, barba, };
        var request = CreateAppointmentRequestDto(barbershop, listServiceOffering);

        _mockBarberRepository.Setup(x => x.GetByBarbershopIdAsync(request.BarbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barber);

        _mockServiceOfferingRepository.Setup(x => x.ListByIdsAsync(request.ServiceIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listServiceOffering);

        _mockAppointmentRepository.Setup(x => x.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockSelectionRepository.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<AppointmentServiceSelection>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<AppointmentRequestedIntegrationEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAppointment(request, client.Id);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
        
        _mockBarberRepository.Verify(x => x.GetByBarbershopIdAsync(request.BarbershopId, It.IsAny<CancellationToken>()), Times.Once);
        _mockServiceOfferingRepository.Verify(x => x.ListByIdsAsync(request.ServiceIds, It.IsAny<CancellationToken>()), Times.Once);
        _mockAppointmentRepository.Verify(
            x => x.AddAsync(
                It.Is<Appointment>(appointment =>
                    appointment.ClientId == client.Id &&
                    appointment.BarberId == barber.Id &&
                    appointment.BarbershopId == barbershop.Id &&
                    appointment.TotalPrice == 90m &&
                    appointment.Status == AppointmentStatus.Pending),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _mockSelectionRepository.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<AppointmentServiceSelection>>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(x => x.PublishAsync(It.IsAny<AppointmentRequestedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static CreateAppointmentRequestDto CreateAppointmentRequestDto(Barbershop barbershop, List<ServiceOffering> serviceOfferings)
    {
        var serviceIds = new List<Guid>
        {
            serviceOfferings[0].Id,
            serviceOfferings[1].Id,
        };

        return new CreateAppointmentRequestDto()
        {
            BarbershopId = barbershop.Id,
            ServiceIds = serviceIds,
            ScheduledAtUtc = DateTime.UtcNow.AddDays(1),
        };
    }
}
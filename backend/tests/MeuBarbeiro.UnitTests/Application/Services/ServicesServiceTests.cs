using FluentAssertions;
using FluentValidation.Results;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.DTOs.Services;
using MeuBarbeiro.Application.Services;
using MeuBarbeiro.Domain.Entities;
using MeuBarbeiro.UnitTests.TestBuilder;
using Moq;

namespace MeuBarbeiro.UnitTests.Application.Services;

public class ServicesServiceTests
{
    private readonly Mock<IBarbershopRepository> _barbershopRepositoryMock = new();
    private readonly Mock<IServiceOfferingRepository> _serviceOfferingRepositoryMock = new();
    private readonly ServicesService _service;

    public ServicesServiceTests()
    {
        _service = new ServicesService(_barbershopRepositoryMock.Object, _serviceOfferingRepositoryMock.Object);
    }

    [Fact]
    public async Task GetServices_DeveRetornarServicos_QuandoBarbeariaExistir()
    {
        // Arrange
        var barbershop = new BarbershopBuilder()
            .WithOwnerUserId(Guid.NewGuid())
            .Build();

        var serviceOffering = new ServiceOfferingBuilder()
            .WithBarbershopId(barbershop.Id)
            .WithName("Corte")
            .WithDescription("Corte de qualidade")
            .WithPrice(50.0m)
            .WithDurationMinutes(40)
            .Build();

        var services = new List<ServiceOffering>() { serviceOffering };

        _barbershopRepositoryMock.Setup(b => b.GetByIdAsync(barbershop.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbershop);

        _serviceOfferingRepositoryMock.Setup(s => s.ListByBarbershopAsync(barbershop.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act 
        var result = await _service.GetServices(barbershop.Id);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ValidationResult.IsValid.Should().BeTrue();
        result.Data.Should().ContainSingle();

        var returnedServices = result.Data.Single();

        returnedServices.BarbershopId.Should().Be(barbershop.Id);
        returnedServices.Name.Should().Be(serviceOffering.Name);
        returnedServices.Description.Should().Be(serviceOffering.Description);
        returnedServices.Price.Should().Be(serviceOffering.Price);
        returnedServices.DurationMinutes.Should().Be(serviceOffering.DurationMinutes);

        _barbershopRepositoryMock.Verify(b => b.GetByIdAsync(barbershop.Id, It.IsAny<CancellationToken>()), Times.Once);
        _serviceOfferingRepositoryMock.Verify(
            s => s.ListByBarbershopAsync(barbershop.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetServices_DeveRetornarListaVazia_QuandoBarbeariaExistirENaoPossuirServicos()
    {
        // Arrange 
        var barbershop = new BarbershopBuilder()
            .WithOwnerUserId(Guid.NewGuid())
            .Build();

        _barbershopRepositoryMock
            .Setup(b => b.GetByIdAsync(barbershop.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbershop);

        _serviceOfferingRepositoryMock.Setup(s => s.ListByBarbershopAsync(barbershop.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act 
        var result = await _service.GetServices(barbershop.Id);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ValidationResult.IsValid.Should().BeTrue();
        result.Data.Should().BeEmpty();

        _barbershopRepositoryMock.Verify(b => b.GetByIdAsync(barbershop.Id, It.IsAny<CancellationToken>()), Times.Once);
        _serviceOfferingRepositoryMock.Verify(
            s => s.ListByBarbershopAsync(barbershop.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetServices_DeveRetornarNotFound_QuandoBarbeariaNaoExistir()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();

        _barbershopRepositoryMock.Setup(b => b.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);

        // Act 
        var result = await _service.GetServices(barbershopId);


        // Assert
        result.Data.Should().BeNull();
        result.IsNotFound.Should().BeTrue();
        result.ValidationResult.IsValid.Should().BeTrue();

        _barbershopRepositoryMock.Verify(b => b.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceOfferingRepositoryMock.Verify(
            s => s.ListByBarbershopAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static CreateServicesRequestDto CreateAddServicesRequestDto()
    {
        return new CreateServicesRequestDto()
        {
            Description = "Corte completo da preferencia do cliente.",
            DurationMinutes = 40,
            Name = "Corte",
            Price = 50.0m
        };
    }
}
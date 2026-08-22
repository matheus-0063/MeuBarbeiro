using FluentAssertions;
using FluentValidation.Results;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.DTOs.Barbershop;
using MeuBarbeiro.Application.Services;
using MeuBarbeiro.Domain.Entities;
using MeuBarbeiro.UnitTests.TestBuilder;
using Moq;

namespace MeuBarbeiro.UnitTests.Application.Services;

public class BarbershopServiceTests
{
    private readonly Mock<IBarbershopRepository> _mockBarbershopRepository;
    private readonly BarbershopService _barbershopService;

    public BarbershopServiceTests()
    {
        _mockBarbershopRepository = new Mock<IBarbershopRepository>();
        _barbershopService = new BarbershopService(_mockBarbershopRepository.Object);
    }
    
    [Fact]
    public async Task SaveBarbershop_DeveCriarBarbearia_QuandoBarbershopIdNaoForInformado()
    {
        // Arrange
        var request = CreateBarbershopRequestDto();
        
        _mockBarbershopRepository
            .Setup(r => r.AddAsync(It.IsAny<Barbershop>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        
        // Act
        var result = await _barbershopService.SaveBarbershop(null, request);
        
        #region Assert
        result.ValidationResult.IsValid.Should().BeTrue();
        
        result.Data.Should().NotBeNull();
        result.Data.Name.Should().Be(request.Name);
        result.Data.City.Should().Be(request.City);
        result.Data.Address.Should().Be(request.Address);
        result.Data.Description.Should().Be(request.Description);
        
        _mockBarbershopRepository.Verify(
            repository => repository.AddAsync(
                It.Is<Barbershop>(barbershop =>
                    barbershop.Name == request.Name &&
                    barbershop.City == request.City &&
                    barbershop.Address == request.Address &&
                    barbershop.Description == request.Description),
                It.IsAny<CancellationToken>()),
            Times.Once);
        #endregion
    }

    [Fact]
    public async Task SaveBarbershop_DeveRetornarFalha_QuandoCriacaoDaBarbeariaForInvalida()
    {
        // Arrange
        var request = CreateBarbershopRequestDto();
        var validationResult = new ValidationResult([new ValidationFailure(nameof(Barbershop.Name), "Nome da barbearia deve ser informado")]);
        
        _mockBarbershopRepository
            .Setup(r => r.AddAsync(It.IsAny<Barbershop>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        // Act
        var result = await _barbershopService.SaveBarbershop(null, request);
        
        // Assert
        result.ValidationResult.IsValid.Should().BeFalse();
        result.ValidationResult.Errors.Should().ContainSingle();
        result.ValidationResult.Errors[0].PropertyName.Should().Be(nameof(Barbershop.Name));
        result.ValidationResult.Errors[0].ErrorMessage.Should().Be("Nome da barbearia deve ser informado");
    }

    [Fact]
    public async Task SaveBarbershop_DeveAtualizarBarbearia_QuandoBarbershopExistir()
    {
        // Arrange
        var request = CreateBarbershopRequestDto();
        
        var barbershop = new BarbershopBuilder()
            .WithOwnerUserId(Guid.NewGuid())
            .WithName("Barbearia Antiga")
            .WithCity("Betim")
            .WithAddress("Rua Padre Lage, 59")
            .WithDescription("Barbearia top")
            .Build();
            
        var barbershopId = barbershop.Id;

        _mockBarbershopRepository
            .Setup(r => r.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbershop);
        
        _mockBarbershopRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Barbershop>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        
        // Act
        var result = await _barbershopService.SaveBarbershop(barbershopId, request);
        
        // Assert
        result.ValidationResult.IsValid.Should().BeTrue();
        result.Data.Should().NotBeNull();
        
        result.Data.Name.Should().Be(request.Name);
        result.Data.City.Should().Be(request.City);
        result.Data.Address.Should().Be(request.Address);
        result.Data.Description.Should().Be(request.Description);
        
        _mockBarbershopRepository.Verify(r => r.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()), Times.Once);
        
        _mockBarbershopRepository.Verify(r => r.UpdateAsync(
            It.Is<Barbershop>(updateBarbeshop => 
                updateBarbeshop.Id == barbershopId &&
                updateBarbeshop.Name == request.Name &&
                updateBarbeshop.City == request.City &&
                updateBarbeshop.Address == request.Address &&
                updateBarbeshop.Description == request.Description), It.IsAny<CancellationToken>()), Times.Once);
        
        _mockBarbershopRepository.Verify(r => r.AddAsync(It.IsAny<Barbershop>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SaveBarbershop_DeveRetornarFalha_QuandoRepositorioFalharAoAtualizarBarbearia()
    {
        // Arrange
        var request = CreateBarbershopRequestDto();
        
        var validationResult = new ValidationResult([new ValidationFailure(nameof(Barbershop.Name), "Nome da barbearia deve ser informado")]);
        
        var barbershop = new BarbershopBuilder()
            .WithOwnerUserId(Guid.NewGuid())
            .WithName("Barbearia Antiga")
            .WithCity("Betim")
            .WithAddress("Rua Padre Lage, 59")
            .WithDescription("Barbearia top")
            .Build();
        
        _mockBarbershopRepository
            .Setup(r => r.GetByIdAsync(barbershop.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbershop);

        _mockBarbershopRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Barbershop>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        // Act 
        var result = await _barbershopService.SaveBarbershop(barbershop.Id, request);
        
        // Assert
        result.ValidationResult.IsValid.Should().BeFalse();
        result.ValidationResult.Errors.Should().ContainSingle();
        
        result.ValidationResult.Errors[0].PropertyName.Should().Be(nameof(Barbershop.Name));
        result.ValidationResult.Errors[0].ErrorMessage.Should().Be("Nome da barbearia deve ser informado");
        
        _mockBarbershopRepository.Verify(
            repository => repository.GetByIdAsync(
                barbershop.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockBarbershopRepository.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<Barbershop>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockBarbershopRepository.Verify(
            repository => repository.AddAsync(
                It.IsAny<Barbershop>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SaveBarbershop_DeveCriarNovaBarbearia_QuandoBarbershopIdNaoExistir()
    {
        // Arrange
        var request = CreateBarbershopRequestDto();
        var barbershopId = Guid.NewGuid();

        _mockBarbershopRepository
            .Setup(repository => repository.GetByIdAsync(
                barbershopId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);

        _mockBarbershopRepository
            .Setup(repository => repository.AddAsync(
                It.IsAny<Barbershop>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _barbershopService.SaveBarbershop(
            barbershopId,
            request);

        // Assert
        result.ValidationResult.IsValid.Should().BeTrue();
        result.Data.Should().NotBeNull();

        result.Data!.Name.Should().Be(request.Name);
        result.Data.City.Should().Be(request.City);
        result.Data.Address.Should().Be(request.Address);
        result.Data.Description.Should().Be(request.Description);

        _mockBarbershopRepository.Verify(
            repository => repository.GetByIdAsync(
                barbershopId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockBarbershopRepository.Verify(
            repository => repository.AddAsync(
                It.Is<Barbershop>(newBarbershop =>
                    newBarbershop.Name == request.Name &&
                    newBarbershop.City == request.City &&
                    newBarbershop.Address == request.Address &&
                    newBarbershop.Description == request.Description),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockBarbershopRepository.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<Barbershop>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetBarbershop_DeveRetornarBarbearia_QuandoEncontrada()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();

        var barbershop = new BarbershopBuilder()
            .WithOwnerUserId(Guid.NewGuid())
            .WithName("Barbearia Antiga")
            .WithCity("Betim")
            .WithAddress("Rua Padre Lage, 59")
            .WithDescription("Barbearia top")
            .Build();
        
        _mockBarbershopRepository
            .Setup(r => r.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbershop);
        
        // Act
        var result = await _barbershopService.GetBarbershop(barbershopId);
        
        // Assert
        result.Should().NotBeNull();
        result.ValidationResult.IsValid.Should().BeTrue();
        
        result.Data.Should().NotBeNull();
        
        result.Data.Name.Should().Be(barbershop.Name);
        result.Data.City.Should().Be(barbershop.City);
        result.Data.Address.Should().Be(barbershop.Address);
        result.Data.Description.Should().Be(barbershop.Description);
        
        _mockBarbershopRepository.Verify(
            r => r.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBarbershop_DeveRetornarNotFound_QuandoBarbeariaNaoExistir()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();

        _mockBarbershopRepository
            .Setup(r => r.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);
        
        // Act
        var result = await _barbershopService.GetBarbershop(barbershopId);
        
        // Assert
        result.ValidationResult.IsValid.Should().BeTrue();
        result.Data.Should().BeNull();       
        
        _mockBarbershopRepository.Verify(r => r.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBarbershops_DeveRetornarTodasAsBarbearias_QuandoCidadeNaoForInformada()
    {
        // Arrange 
        _mockBarbershopRepository
            .Setup(r => r.ListAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Barbershop>());
        
        // Act
        var barbershops = await _barbershopService.GetBarbershops();
        
        // Assert
        barbershops.Should().NotBeNull();
        barbershops.Data.Should().NotBeNull();
        
        _mockBarbershopRepository.Verify(r => r.ListAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBarbershops_DeveRetornarBarbeariasDaCidadeInformada()
    {
        // Arrange
        var cidade = "Betim";
        
        var barbershop = new BarbershopBuilder()
            .WithOwnerUserId(Guid.NewGuid())
            .WithName("Barbearia Antiga")
            .WithCity("Betim")
            .WithAddress("Rua Padre Lage, 59")
            .WithDescription("Barbearia top")
            .Build();
        
        var barbershops = new List<Barbershop> { barbershop };
        
        _mockBarbershopRepository
            .Setup(r => r.ListAsync(cidade, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbershops);
        
        // Act
        var result = await _barbershopService.GetBarbershops(cidade);
        
        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainSingle();
        
        result.ValidationResult.IsValid.Should().BeTrue();

        var returnedBarbershop = result.Data.Single();
        
        returnedBarbershop.Name.Should().Be(barbershop.Name);
        returnedBarbershop.City.Should().Be(barbershop.City);
        returnedBarbershop.Address.Should().Be(barbershop.Address);
        returnedBarbershop.Description.Should().Be(barbershop.Description);
        
        _mockBarbershopRepository.Verify(r => r.ListAsync(cidade, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBarbershops_DeveRetornarListaVazia_QuandoNaoExistiremBarbearias()
    {
        // Arrange
        var cidade = "Betim";
        
        _mockBarbershopRepository
            .Setup(r => r.ListAsync(cidade, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        
        // Act
        var result = await _barbershopService.GetBarbershops(cidade);
        
        // Assert
        result.Should().NotBeNull();
        
        result.ValidationResult.IsValid.Should().BeTrue();
        
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
        
        _mockBarbershopRepository.Verify(r => r.ListAsync(cidade, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static CreateBarbershopRequestDto CreateBarbershopRequestDto()
    {
        return new CreateBarbershopRequestDto
        {
            OwnerUserId = Guid.NewGuid(),
            Name = "Barbearia BH",
            City = "Belo Horizonte",
            Address = "R. Ilacir Pereira Lima, 539 - Silveira, Belo Horizonte - MG, 31140-540",
            Description = "Melhor barbearia de Belo Horizonte",
        };
    }
}
using System.Text;
using System.Text.Json;
using FluentAssertions;
using MeuBarbeiro.Application.Caching;
using MeuBarbeiro.Application.DTOs.Barbershop;
using MeuBarbeiro.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeuBarbeiro.UnitTests.Infrastructure.Caching;

public class RedisCacheServiceTests
{
    private const string BarberShopId = "d7bf727f-3a86-49ac-b457-ac979db251d9";
    
    private readonly Mock<IDistributedCache> _distributedCacheMock;
    private readonly Mock<ILogger<RedisCacheService>> _loggerMock;
    private readonly RedisCacheService _redisCacheService;

    public RedisCacheServiceTests()
    {
        _distributedCacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<RedisCacheService>>();
        
        _redisCacheService = new RedisCacheService(_distributedCacheMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task GetAsync_DeveRetornarObjetoDesserializado_QuandoChaveExistirNoCache()
    {
        // Arrange 
        var expected = GenerateBarbershopResponseDto();
        var key = CacheKeys.Barbershop(expected.Id);

        var json = JsonSerializer.Serialize(expected,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        
        _distributedCacheMock
            .Setup(cache => cache.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(json));
        
        // Act
        var cached = await _redisCacheService.GetAsync<BarbershopResponseDto>(key);
        
        // Assert
        cached.Should().NotBeNull();
        cached.Should().BeEquivalentTo(expected);
        
        _distributedCacheMock
            .Verify(cache => cache.GetAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_DeveRetornarNull_QuandoChaveNaoExistirNoCache()
    {
        // Arrange
        var key = CacheKeys.Barbershop(Guid.NewGuid());

        _distributedCacheMock
            .Setup(cache => cache.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
        
        // Act
        var cached = await _redisCacheService.GetAsync<BarbershopResponseDto>(key);
        
        // Assert
        cached.Should().BeNull();
        
        _distributedCacheMock
            .Verify(cache => cache.GetAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_DeveRetornarNull_QuandoRedisLancarExcecao()
    {
        // Arrange
        var key = CacheKeys.Barbershop(Guid.NewGuid());
        
        _distributedCacheMock
            .Setup(cache => cache.GetAsync(key, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro ao obter dados do cache"));
        
        // Act
        var cached = _redisCacheService.GetAsync<BarbershopResponseDto>(key);
        
        // Assert
        cached.Should().BeNull();
        
        _distributedCacheMock
            .Verify(cache => cache.GetAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetAsync_DeveSerializarObjetoEArmazenarComTtlInformado()
    {
        // Arrange
        var expected = GenerateBarbershopResponseDto();
        var key = CacheKeys.Barbershop(expected.Id);
        var ttl = TimeSpan.FromMinutes(5);

        byte[]? storedBytes = null;
        DistributedCacheEntryOptions? storedOptions = null;

        _distributedCacheMock
            .Setup(cache => cache.SetAsync(
                key,
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback((
                string capturedKey,
                byte[] bytes,
                DistributedCacheEntryOptions options,
                CancellationToken cancellationToken) =>
            {
                storedBytes = bytes;
                storedOptions = options;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _redisCacheService.SetAsync(key, expected, ttl);

        // Assert
        storedBytes.Should().NotBeNull();
        storedOptions.Should().NotBeNull();

        var json = Encoding.UTF8.GetString(storedBytes!);
        var actual = JsonSerializer.Deserialize<BarbershopResponseDto>(json,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        actual.Should().BeEquivalentTo(expected);
        storedOptions!.AbsoluteExpirationRelativeToNow.Should().Be(ttl);

        _distributedCacheMock
            .Verify(cache => cache.SetAsync(key, It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task SetAsync_NaoDeveLancarExcecao_QuandoRedisEstiverIndisponivel()
    {
    }

    [Fact]
    public async Task RemoveAsync_DeveRemoverChaveDoCache()
    {
    }

    [Fact]
    public async Task RemoveAsync_NaoDeveLancarExcecao_QuandoRedisEstiverIndisponivel()
    {
    }

    private BarbershopResponseDto GenerateBarbershopResponseDto()
    {
        return new BarbershopResponseDto
        {
            Id = Guid.Parse(BarberShopId),
            Address = "Rua Padre Lage, 59",
            City = "Betim",
            Name = "Barbearia Betim",
            AverageRating = 4.8d,
        };
    }
}
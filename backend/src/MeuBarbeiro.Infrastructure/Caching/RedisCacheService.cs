using System.Text.Json;
using MeuBarbeiro.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace MeuBarbeiro.Infrastructure.Caching;

public class RedisCacheService(IDistributedCache distributedCache, ILogger<RedisCacheService> logger)
    : ICacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = 
        new (JsonSerializerDefaults.Web);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        try
        {
            var json = await distributedCache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrEmpty(json))
            {
                logger.LogDebug("Cache miss para a chave {CacheKey}", key);
                return default;
            }

            var value = JsonSerializer.Deserialize<T>(json, SerializerOptions);
            if (value == null)
            {
                logger.LogDebug("O cache continha um valor nulo ou inválido para a chave {CacheKey}", key);
                await Remove(key, cancellationToken);
                return default;
            }

            logger.LogDebug("Cache hit para a chave {CacheKey}", key);
            return value;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao consultar o cache para a chave {CacheKey}. A aplicação seguirá sem cache.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (ttl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ttl), "O TTL do cache deve ser maior que zero.");
        }

        try
        {
            var json = JsonSerializer.Serialize(value, SerializerOptions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };

            await distributedCache.SetStringAsync(key, json, options, cancellationToken);

            logger.LogDebug("Valor armazenado no cache para a chave {CacheKey} com TTL de {CacheTtl}.", key, ttl);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao gravar no cache {CacheKey}. A aplicação seguirá sem cache.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        try
        {
            await Remove(key, cancellationToken);
            logger.LogDebug("Chave {CacheKey} removida do cache", key);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao remover {CacheKey} do cache. Ela expirará pelo TTL.", key);
        }
    }

    private async Task Remove(string key, CancellationToken cancellationToken = default)
    {
        await distributedCache.RemoveAsync(key, cancellationToken);
    }
}
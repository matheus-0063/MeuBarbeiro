namespace MeuBarbeiro.Application.Caching;

public static class CacheKeys
{
    public static string Barbershop(Guid id) => 
        $"catalog:barbershop:{id}";
    
    public static string BarbershopServices(Guid barbershopId) => 
        $"catalog:barbershop:{barbershopId}:services";
}
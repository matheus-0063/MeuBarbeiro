using MeuBarbeiro.Application.DTOs.Barbershop;
using MeuBarbeiro.Application.DTOs.Shared;

namespace MeuBarbeiro.Application.Abstractions.Services;

public interface IBarbershopService
{
    Task<ServiceResult<BarbershopResponseDto>> SaveBarbershop(Guid? barbershopId, CreateBarbershopRequestDto request, CancellationToken cancellationToken = default);
    Task<ServiceResult<BarbershopResponseDto>> GetBarbershop(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<IEnumerable<BarbershopResponseDto>>> GetBarbershops(string? city = null, CancellationToken cancellationToken = default);
}

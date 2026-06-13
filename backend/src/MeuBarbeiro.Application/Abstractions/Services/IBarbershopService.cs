using MeuBarbeiro.Application.DTOs.Barbershop;
using MeuBarbeiro.Application.DTOs.Shared;

namespace MeuBarbeiro.Application.Abstractions.Services;

public interface IBarbershopService
{
    Task<ServiceResult<Guid>> CreateBarbershop(CreateBarbershopRequestDto request);
    Task<ServiceResult<BarbershopResponseDto>> GetBarbershop(Guid id);
    Task<ServiceResult<IEnumerable<BarbershopResponseDto>>> GetBarbershops(string? city = null, CancellationToken cancellationToken = default);
}

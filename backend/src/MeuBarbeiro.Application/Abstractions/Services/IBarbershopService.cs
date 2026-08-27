using MeuBarbeiro.Application.DTOs.Barbers;
using MeuBarbeiro.Application.DTOs.Barbershop;
using MeuBarbeiro.Application.DTOs.Shared;

namespace MeuBarbeiro.Application.Abstractions.Services;

public interface IBarbershopService
{
    Task<ServiceResult<BarbershopResponseDto>> CreateBarbershop(CreateBarbershopRequestDto request,
        Guid barbershopOwnerId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateBarbershop(UpdateBarbershopRequestDto request, Guid barbershopId, Guid barbershopOwnerId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> LinkBaberToTheBarbershop(Guid barbershopId, Guid barberId, Guid barbershopOwnerId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> RemoveBarberToBarbershop(Guid barbershopId, Guid barberId, Guid barbershopOwnerId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IEnumerable<BarberResponseDto>>> ListBarbersToBarbershop(Guid barbershopId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<BarbershopResponseDto>> GetBarbershop(Guid id,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IEnumerable<BarbershopResponseDto>>> ListBarbershopsToOwner(Guid ownerId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IEnumerable<BarbershopResponseDto>>> GetBarbershops(string? city = null,
        CancellationToken cancellationToken = default);
}
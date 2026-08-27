using MeuBarbeiro.Application.DTOs.Services;
using MeuBarbeiro.Application.DTOs.Shared;

namespace MeuBarbeiro.Application.Abstractions.Services;

public interface IServicesService
{
    Task<ServiceResult<ServiceResponseDto>> CreateService(CreateServicesRequestDto request, Guid barbershopOwnerId,
        Guid barbershopId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<ServiceResponseDto>> UpdateService(UpdateServicesRequestDto request,
        Guid barbershopOwnerId,
        Guid barbershopId,
        Guid serviceId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<ServiceResponseDto>> GetService(Guid serviceId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IEnumerable<ServiceResponseDto>>> GetServices(Guid barbershopId,
        CancellationToken cancellationToken = default);
}
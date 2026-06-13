using FluentValidation.Results;
using MeuBarbeiro.Application.DTOs.Services;
using MeuBarbeiro.Application.DTOs.Shared;

namespace MeuBarbeiro.Application.Abstractions.Services;

public interface IServicesService
{
    Task<ServiceResult<Guid>> AddServices(AddServicesRequestDto request);
    Task<ServiceResult<IEnumerable<ServiceResponseDto>>> GetServices(Guid barbershopId, CancellationToken cancellationToken = default);
}

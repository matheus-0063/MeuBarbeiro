using FluentValidation.Results;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Application.DTOs.Services;
using MeuBarbeiro.Application.DTOs.Shared;
using MeuBarbeiro.Application.Mappings.Services;

namespace MeuBarbeiro.Application.Services;

public class ServicesService(IBarbershopRepository barbershopRepository, IServiceOfferingRepository serviceOfferingRepository) 
    : IServicesService
{
    public async Task<ServiceResult<ServiceResponseDto>> AddServices(AddServicesRequestDto request, Guid barbershopOwnerId,
        CancellationToken cancellationToken = default)
    {
        var barbershops = await barbershopRepository.GetByBarbershopOwnerIdAsync(barbershopOwnerId, cancellationToken);
        if (barbershops == null || !barbershops.Any()) return ServiceResult<ServiceResponseDto>.NotFound();
        
        foreach (var barbershop in barbershops)
        {
            barbershop.
        }
    }

    public async Task<ServiceResult<IEnumerable<ServiceResponseDto>>> GetServices(Guid barbershopId,
        CancellationToken cancellationToken = default)
    {
        var barbershop = await barbershopRepository.GetByIdAsync(barbershopId, cancellationToken);
        if (barbershop == null) return ServiceResult<IEnumerable<ServiceResponseDto>>.NotFound();

        var services = await serviceOfferingRepository.ListByBarbershopAsync(barbershopId, cancellationToken);
        var response = services.Select(service => service.ToResponseDto());

        return ServiceResult<IEnumerable<ServiceResponseDto>>.Success(response);
    }
}
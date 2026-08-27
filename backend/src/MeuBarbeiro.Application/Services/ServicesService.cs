using FluentValidation.Results;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Application.DTOs.Services;
using MeuBarbeiro.Application.DTOs.Shared;
using MeuBarbeiro.Application.Mappings.Services;
using MeuBarbeiro.Domain.Entities;
using MeuBarbeiro.Domain.Exceptions;

namespace MeuBarbeiro.Application.Services;

public class ServicesService(
    IBarbershopRepository barbershopRepository,
    IServiceOfferingRepository serviceOfferingRepository)
    : IServicesService
{
    public async Task<ServiceResult<ServiceResponseDto>> CreateService(CreateServicesRequestDto request,
        Guid barbershopOwnerId, Guid barbershopId,
        CancellationToken cancellationToken = default)
    {
        var barbershop = await barbershopRepository.GetByIdAsync(barbershopId, cancellationToken);

        if (barbershop == null)
            return ServiceResult<ServiceResponseDto>.NotFound();

        if (barbershop.OwnerUserId != barbershopOwnerId)
            return ServiceResult<ServiceResponseDto>.Forbidden();

        ServiceOffering serviceOffering;

        try
        {
            serviceOffering = request.ToEntity(barbershopId);
        }
        catch (ServiceOfferingValidationException ex)
        {
            return DomainFailure(ex);
        }

        await serviceOfferingRepository.AddAsync(serviceOffering, cancellationToken);
        return ServiceResult<ServiceResponseDto>.Success(serviceOffering.ToResponseDto());
    }

    public async Task<ServiceResult<ServiceResponseDto>> UpdateService(UpdateServicesRequestDto request,
        Guid barbershopOwnerId,
        Guid barbershopId,
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        var barbershop = await barbershopRepository.GetByIdAsync(barbershopId, cancellationToken);

        if (barbershop == null)
            return ServiceResult<ServiceResponseDto>.NotFound();

        if (barbershop.OwnerUserId != barbershopOwnerId)
            return ServiceResult<ServiceResponseDto>.Forbidden();

        var service = await serviceOfferingRepository.GetByIdAsync(serviceId, cancellationToken);

        if (service == null)
            return ServiceResult<ServiceResponseDto>.NotFound();

        if (service.BarbershopId != barbershopId)
            return ServiceResult<ServiceResponseDto>.NotFound();

        try
        {
            service.UpdateDetails(request.Name, request.Description, request.Price, request.DurationMinutes);
        }
        catch (ServiceOfferingValidationException ex)
        {
            return DomainFailure(ex);
        }
        
        await serviceOfferingRepository.UpdateAsync(service, cancellationToken);
        return ServiceResult<ServiceResponseDto>.Success(service.ToResponseDto());
    }

    public async Task<ServiceResult<ServiceResponseDto>> GetService(Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        var service = await serviceOfferingRepository.GetByIdAsync(serviceId, cancellationToken);
        if (service == null) return ServiceResult<ServiceResponseDto>.NotFound();

        return ServiceResult<ServiceResponseDto>.Success(service.ToResponseDto());
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

    private static ServiceResult<ServiceResponseDto> DomainFailure(ServiceOfferingValidationException exception)
    {
        var validationResults = new ValidationResult();

        validationResults.Errors.Add(new ValidationFailure(
            exception.PropertyName,
            exception.Message)
        );

        return ServiceResult<ServiceResponseDto>.Failure(validationResults);
    }
}
using FluentValidation.Results;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Application.DTOs.Services;
using MeuBarbeiro.Application.DTOs.Shared;
using MeuBarbeiro.Application.Mappings.Services;

namespace MeuBarbeiro.Application.Services;

public class ServicesService(
    IBarbershopRepository barbershopRepository,
    IServiceOfferingRepository serviceOfferingRepository) : IServicesService
{
    public async Task<ServiceResult<Guid>> AddServices(AddServicesRequestDto request)
    {
        var barbershop = await barbershopRepository.GetByIdAsync(request.BarbershopId);
        if (barbershop == null)
        {
            var validationResult = new ValidationResult();
            validationResult.Errors.Add(new ValidationFailure(nameof(request.BarbershopId), "Barbearia nao encontrada."));
            return ServiceResult<Guid>.Failure(validationResult);
        }

        var serviceOffering = request.ToEntity();
        var validation = await serviceOfferingRepository.AddAsync(serviceOffering);

        return validation.IsValid
            ? ServiceResult<Guid>.Success(serviceOffering.Id)
            : ServiceResult<Guid>.Failure(validation);
    }

    public async Task<ServiceResult<IEnumerable<ServiceResponseDto>>> GetServices(Guid barbershopId, CancellationToken cancellationToken = default)
    {
        var barbershop = await barbershopRepository.GetByIdAsync(barbershopId, cancellationToken);
        if (barbershop == null)
        {
            return ServiceResult<IEnumerable<ServiceResponseDto>>.NotFound();
        }

        var services = await serviceOfferingRepository.ListByBarbershopAsync(barbershopId, cancellationToken);
        var response = services.Select(service => service.ToResponseDto());

        return ServiceResult<IEnumerable<ServiceResponseDto>>.Success(response);
    }
}

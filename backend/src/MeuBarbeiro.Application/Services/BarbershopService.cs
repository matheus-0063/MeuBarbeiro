using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Application.DTOs.Barbershop;
using MeuBarbeiro.Application.DTOs.Shared;
using MeuBarbeiro.Application.Mappings.Barbershops;

namespace MeuBarbeiro.Application.Services;

public class BarbershopService(IBarbershopRepository barbershopRepository) : IBarbershopService
{
    public async Task<ServiceResult<BarbershopResponseDto>> SaveBarbershop(Guid? barbershopId, CreateBarbershopRequestDto request, CancellationToken cancellationToken = default)
    {
        if (barbershopId.HasValue)
        {
            var existingBarbershop = await barbershopRepository.GetByIdAsync(barbershopId.Value, cancellationToken);
            if (existingBarbershop is not null)
            {
                existingBarbershop.UpdateDetails(request.Name, request.City, request.Address, request.Description);
                var updateValidationResult = await barbershopRepository.UpdateAsync(existingBarbershop, cancellationToken);

                if (!updateValidationResult.IsValid)
                {
                    return ServiceResult<BarbershopResponseDto>.Failure(updateValidationResult);
                }

                return ServiceResult<BarbershopResponseDto>.Success(existingBarbershop.ToResponseDto());
            }
        }

        var barbershop = request.ToEntity();
        var validationResult = await barbershopRepository.AddAsync(barbershop, cancellationToken);

        if (!validationResult.IsValid)
        {
            return ServiceResult<BarbershopResponseDto>.Failure(validationResult);
        }

        return ServiceResult<BarbershopResponseDto>.Success(barbershop.ToResponseDto());
    }

    public async Task<ServiceResult<BarbershopResponseDto>> GetBarbershop(Guid id, CancellationToken cancellationToken = default)
    {
        var barbershop = await barbershopRepository.GetByIdAsync(id, cancellationToken);

        return barbershop == null
            ? ServiceResult<BarbershopResponseDto>.NotFound()
            : ServiceResult<BarbershopResponseDto>.Success(barbershop.ToResponseDto());
    }

    public async Task<ServiceResult<IEnumerable<BarbershopResponseDto>>> GetBarbershops(string? city = null, CancellationToken cancellationToken = default)
    {
        var barbershops = await barbershopRepository.ListAsync(city, cancellationToken);
        var response = barbershops.Select(barbershop => barbershop.ToResponseDto());

        return ServiceResult<IEnumerable<BarbershopResponseDto>>.Success(response);
    }
}

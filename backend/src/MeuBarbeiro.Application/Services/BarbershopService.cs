using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Application.DTOs.Barbershop;
using MeuBarbeiro.Application.DTOs.Shared;
using MeuBarbeiro.Application.Mappings.Barbershops;

namespace MeuBarbeiro.Application.Services;

public class BarbershopService(IBarbershopRepository barbershopRepository) : IBarbershopService
{
    public async Task<ServiceResult<Guid>> CreateBarbershop(CreateBarbershopRequestDto request)
    {
        var barbershop = request.ToEntity();
        var validationResult = await barbershopRepository.AddAsync(barbershop);

        if (!validationResult.IsValid) 
            return ServiceResult<Guid>.Failure(validationResult);
        
        return ServiceResult<Guid>.Success(barbershop.Id);
    }

    public async Task<ServiceResult<BarbershopResponseDto>> GetBarbershop(Guid id)
    {
        var barbershop = await barbershopRepository.GetByIdAsync(id);

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

using FluentValidation.Results;
using MeuBarbeiro.Application.Abstractions.Caching;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Application.Caching;
using MeuBarbeiro.Application.DTOs.Barbers;
using MeuBarbeiro.Application.DTOs.Barbershop;
using MeuBarbeiro.Application.DTOs.Services;
using MeuBarbeiro.Application.DTOs.Shared;
using MeuBarbeiro.Application.Mappings.Barbers;
using MeuBarbeiro.Application.Mappings.Barbershops;
using MeuBarbeiro.Domain.Exceptions;

namespace MeuBarbeiro.Application.Services;

public class BarbershopService(
    IBarbershopRepository barbershopRepository,
    IBarberRepository barberRepository,
    IUserRepository userRepository,
    ICacheService cacheService)
    : IBarbershopService
{
    public async Task<ServiceResult<BarbershopResponseDto>> CreateBarbershop(CreateBarbershopRequestDto request,
        Guid barbershopOwnerId,
        CancellationToken cancellationToken = default)
    {
        var barbershop = request.ToEntity(barbershopOwnerId);
        await barbershopRepository.AddAsync(barbershop, cancellationToken);

        return ServiceResult<BarbershopResponseDto>.Success(barbershop.ToResponseDto());
    }

    public async Task<ServiceResult> UpdateBarbershop(UpdateBarbershopRequestDto request, Guid barbershopId,
        Guid barbershopOwnerId,
        CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.Barbershop(barbershopId);
        var barbershop = await barbershopRepository.GetByIdAsync(barbershopId, cancellationToken);

        if (barbershop == null) return ServiceResult.NotFound();
        if (barbershop.OwnerUserId != barbershopOwnerId) return ServiceResult.Forbidden();

        barbershop.UpdateDetails(request.Name, request.City, request.Address, request.Description);

        await barbershopRepository.UpdateAsync(barbershop, cancellationToken);
        await cacheService.RemoveAsync(key, cancellationToken);
        
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> LinkBaberToTheBarbershop(Guid barbershopId, Guid barberId, Guid barbershopOwnerId,
        CancellationToken cancellationToken = default)
    {
        var barbershop = await barbershopRepository.GetByIdAsync(barbershopId, cancellationToken);

        if (barbershop == null) return ServiceResult.NotFound();
        if (barbershop.OwnerUserId != barbershopOwnerId) return ServiceResult.Forbidden();

        var barber = await barberRepository.GetByIdAsync(barberId, cancellationToken);
        if (barber == null) return ServiceResult.NotFound();

        try
        {
            barber.AssignBarbershop(barbershopId);
        }
        catch (BarberBelongsAnotherBarbershopException ex)
        {
            return DomainFailure("barberId", ex.Message);
        }

        await barberRepository.UpdateAsync(barber, cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RemoveBarberToBarbershop(Guid barbershopId, Guid barberId, Guid barbershopOwnerId,
        CancellationToken cancellationToken = default)
    {
        var barbershop = await barbershopRepository.GetByIdAsync(barbershopId, cancellationToken);

        if (barbershop == null) return ServiceResult.NotFound();
        if (barbershop.OwnerUserId != barbershopOwnerId) return ServiceResult.Forbidden();

        var barber = await barberRepository.GetByIdAsync(barberId, cancellationToken);
        if (barber == null) return ServiceResult.NotFound();

        try
        {
            barber.RemoveFromBarbershop(barbershopId);
        }
        catch (BarberDoesNotBelongBarbershopException ex)
        {
            return DomainFailure("barberId", ex.Message);
        }

        await barberRepository.UpdateAsync(barber, cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult<IEnumerable<BarbershopResponseDto>>> ListBarbershopsToOwner(Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        var barbershops = await barbershopRepository.GetByBarbershopOwnerIdAsync(ownerId, cancellationToken);
        if (barbershops == null) return ServiceResult<IEnumerable<BarbershopResponseDto>>.NotFound();

        var response = barbershops.Select(b => b.ToResponseDto());
        return ServiceResult<IEnumerable<BarbershopResponseDto>>.Success(response);
    }

    // Entender melhor
    public async Task<ServiceResult<IEnumerable<BarberResponseDto>>> ListBarbersToBarbershop(Guid barbershopId,
        CancellationToken cancellationToken = default)
    {
        var validationResults = new ValidationResult();
        
        var barbershop = await barbershopRepository.GetByIdAsync(barbershopId, cancellationToken);
        if (barbershop == null) return ServiceResult<IEnumerable<BarberResponseDto>>.NotFound();

        var barbers = await barberRepository.ListByBarbershopAsync(barbershopId, cancellationToken);
        if (!barbers.Any()) return ServiceResult<IEnumerable<BarberResponseDto>>.Success([]);

        var userIds = barbers.Select(b => b.UserId).Distinct();

        var users = await userRepository.ListByIdsAsync(userIds, cancellationToken);
        if (!users.Any()) return ServiceResult<IEnumerable<BarberResponseDto>>.NotFound();

        var usersById = users.ToDictionary(user => user.Id);
        
        var userIdsNotFound = barbers
            .Select(barber => barber.UserId)
            .Where(userId => !usersById.ContainsKey(userId))
            .Distinct()
            .ToArray();

        if (userIdsNotFound.Length > 0)
        {
            validationResults.Errors.Add(new ValidationFailure("barberId", "Barbeiro nao tem usuario atrelado"));
            return ServiceResult<IEnumerable<BarberResponseDto>>.Failure(validationResults);
        }

        var response = barbers
            .Select(barber => barber.ToDto(usersById[barber.UserId].Name))
            .ToList();

        return ServiceResult<IEnumerable<BarberResponseDto>>.Success(response);
    }

    public async Task<ServiceResult<BarbershopResponseDto>> GetBarbershop(Guid id,
        CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.Barbershop(id);
        
        var cached = await cacheService.GetAsync<BarbershopResponseDto>(key, cancellationToken);
        if (cached != null) 
            return ServiceResult<BarbershopResponseDto>.Success(cached);
        
        var barbershop = await barbershopRepository.GetByIdAsync(id, cancellationToken);
        if (barbershop == null) 
            return ServiceResult<BarbershopResponseDto>.NotFound();

        var barbershopResponse = barbershop.ToResponseDto();
        await cacheService.SetAsync(key, barbershopResponse, TimeSpan.FromMinutes(5), cancellationToken);
        
        return ServiceResult<BarbershopResponseDto>.Success(barbershopResponse);
    }

    public async Task<ServiceResult<IEnumerable<BarbershopResponseDto>>> GetBarbershops(string? city = null,
        CancellationToken cancellationToken = default)
    {
        var barbershops = await barbershopRepository.ListAsync(city, cancellationToken);
        var response = barbershops.Select(barbershop => barbershop.ToResponseDto());

        return ServiceResult<IEnumerable<BarbershopResponseDto>>.Success(response);
    }

    private static ServiceResult<ServiceResponseDto> DomainFailure(string propertyName, string message)
    {
        var validationResults = new ValidationResult();

        validationResults.Errors.Add(new ValidationFailure(propertyName, message));
        return ServiceResult<ServiceResponseDto>.Failure(validationResults);
    }
}
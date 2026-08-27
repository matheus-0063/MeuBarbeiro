using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Application.DTOs.Barbers;
using MeuBarbeiro.Application.DTOs.Barbershop;
using MeuBarbeiro.Application.DTOs.Shared;
using MeuBarbeiro.Application.Mappings.Barbers;
using MeuBarbeiro.Application.Mappings.Barbershops;

namespace MeuBarbeiro.Application.Services;

public class BarbershopService(
    IBarbershopRepository barbershopRepository,
    IBarberRepository barberRepository,
    IUserRepository userRepository)
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
        var barbershop = await barbershopRepository.GetByIdAsync(barbershopId, cancellationToken);

        if (barbershop == null) return ServiceResult.NotFound();
        if (barbershop.OwnerUserId != barbershopOwnerId) return ServiceResult.Forbidden();

        barbershop.UpdateDetails(request.Name, request.City, request.Address, request.Description);

        await barbershopRepository.UpdateAsync(barbershop, cancellationToken);
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

        barber.AssignBarbershop(barbershopId);

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

        barber.RemoveFromBarbershop(barbershopId);

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
        var barbershop = await barbershopRepository.GetByIdAsync(barbershopId, cancellationToken);
        if (barbershop == null) return ServiceResult<IEnumerable<BarberResponseDto>>.NotFound();

        var barbers = await barberRepository.ListByBarbershopAsync(barbershopId, cancellationToken);
        if (!barbers.Any()) return ServiceResult<IEnumerable<BarberResponseDto>>.NotFound();

        var userIds = barbers.Select(b => b.UserId).Distinct();

        var users = await userRepository.ListByIdsAsync(userIds, cancellationToken);
        if (!users.Any()) return ServiceResult<IEnumerable<BarberResponseDto>>.NotFound();

        var usersById = users.ToDictionary(
            user => user.Id,
            user => user
        );

        var response = barbers.Select(barber =>
        {
            var user = usersById[barber.UserId];
            return barber.ToDto(user.Name);
        }).ToList();

        return ServiceResult<IEnumerable<BarberResponseDto>>.Success(response);
    }

    public async Task<ServiceResult<BarbershopResponseDto>> GetBarbershop(Guid id,
        CancellationToken cancellationToken = default)
    {
        var barbershop = await barbershopRepository.GetByIdAsync(id, cancellationToken);

        return barbershop == null
            ? ServiceResult<BarbershopResponseDto>.NotFound()
            : ServiceResult<BarbershopResponseDto>.Success(barbershop.ToResponseDto());
    }

    public async Task<ServiceResult<IEnumerable<BarbershopResponseDto>>> GetBarbershops(string? city = null,
        CancellationToken cancellationToken = default)
    {
        var barbershops = await barbershopRepository.ListAsync(city, cancellationToken);
        var response = barbershops.Select(barbershop => barbershop.ToResponseDto());

        return ServiceResult<IEnumerable<BarbershopResponseDto>>.Success(response);
    }
}
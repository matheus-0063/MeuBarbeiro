using MeuBarbeiro.Application.DTOs.Auth;
using MeuBarbeiro.Application.DTOs.Barbers;
using MeuBarbeiro.Application.DTOs.Clients;

namespace MeuBarbeiro.Application.Abstractions.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterClientAsync(RegisterClientRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> RegisterBarberAsync(RegisterBarberRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> RegisterBarbeshopOwnerAsync(RegisterBarbershopOwnerRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(LoginRequest request,
        CancellationToken cancellationToken = default);
}
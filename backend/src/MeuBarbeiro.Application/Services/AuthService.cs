using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.DTOs.Auth;
using MeuBarbeiro.Application.DTOs.Barbers;
using MeuBarbeiro.Application.DTOs.Clients;
using MeuBarbeiro.Domain.Entities;
using MeuBarbeiro.Domain.Enums;

namespace MeuBarbeiro.Application.Services;

public class AuthService(IUserRepository userRepository, IClientRepository clientRepository, IBarberRepository barberRepository, 
    IPasswordHasherService passwordHasher, IJwtTokenService jwtTokenService)
{

    public async Task<AuthResponse> RegisterClientAsync(RegisterClientRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null) throw new Exception($"Email {request.Email} já cadastrado");
        
        var passwordHash = passwordHasher.Hash(request.Password);
        
        var user = new User(request.Name, request.Email, passwordHash, UserRole.Client);
        await userRepository.AddAsync(user, cancellationToken);

        var client = new Client(user.Id);
        await clientRepository.AddAsync(client, cancellationToken);

        var token = jwtTokenService.GenerateToken(user);

        return new AuthResponse
        {
            AccessToken = token,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task<AuthResponse> RegisterBarberAsync(RegisterBarberRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null) throw new Exception($"Email {request.Email} já cadastrado");
        
        var passwordHash = passwordHasher.Hash(request.Password);
        
        var user = new User(request.Name, request.Email, passwordHash, UserRole.Barber);
        await userRepository.AddAsync(user, cancellationToken);
        
        var barber = new Barber(user.Id, request.BarbershopId);
        await barberRepository.AddAsync(barber, cancellationToken);
        
        var token = jwtTokenService.GenerateToken(user);
        
        return new AuthResponse
        {
            AccessToken = token,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null) throw new Exception("E-mail não cadastrado");

        var passwordIsValid = passwordHasher.VerifyPasswordHash(request.Password, user.PasswordHash);
        if (!passwordIsValid) throw new Exception("E-mail ou senha inválidos.");
        
        var token = jwtTokenService.GenerateToken(user);

        return new AuthResponse
        {
            AccessToken = token,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}

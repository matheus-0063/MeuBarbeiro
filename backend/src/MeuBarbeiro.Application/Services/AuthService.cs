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
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly IBarberRepository _barberRepository = barberRepository;
    private readonly IPasswordHasherService _passwordHasher = passwordHasher;
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService;

    public async Task<AuthResponse> RegisterClientAsync(RegisterClientRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser is not null) throw new Exception($"Email {request.Email} já cadastrado");
        
        var passwordHash = _passwordHasher.Hash(request.Password);
        
        var user = new User(request.Name, request.Email, passwordHash, UserRole.Client);
        await _userRepository.AddAsync(user);

        var client = new Client(user.Id);
        await _clientRepository.AddAsync(client);

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponse
        {
            AccessToken = token,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task<AuthResponse> RegisterBarberAsync(RegisterBarberRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser is not null) throw new Exception($"Email {request.Email} já cadastrado");
        
        var passwordHash = _passwordHasher.Hash(request.Password);
        
        var user = new User(request.Name, request.Email, passwordHash, UserRole.Barber);
        await _userRepository.AddAsync(user);
        
        var barber = new Barber(user.Id, request.BarbershopId);
        await _barberRepository.AddAsync(barber);
        
        var token = _jwtTokenService.GenerateToken(user);
        
        return new AuthResponse
        {
            AccessToken = token,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null) throw new Exception("E-mail não cadastrado");

        var passwordIsValid = _passwordHasher.VerifyPasswordHash(request.Password, user.PasswordHash);
        if (!passwordIsValid) throw new Exception("E-mail ou senha inválidos.");
        
        var token = _jwtTokenService.GenerateToken(user);

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

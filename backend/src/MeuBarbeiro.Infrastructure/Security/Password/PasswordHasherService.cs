using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MeuBarbeiro.Infrastructure.Security.Password;

public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string Hash(string password)
    {
        return _passwordHasher.HashPassword(null!, password);
    }

    public bool VerifyPasswordHash(string password, string passwordHash)
    {
        var result = _passwordHasher.VerifyHashedPassword(null!, passwordHash, password);
        return result == PasswordVerificationResult.Success;
    }
}

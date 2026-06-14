using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Abstractions.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
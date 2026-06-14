namespace MeuBarbeiro.Application.Abstractions.Services;

public interface IPasswordHasherService
{
    string Hash(string password);
    bool VerifyPasswordHash(string password, string passwordHash);
}
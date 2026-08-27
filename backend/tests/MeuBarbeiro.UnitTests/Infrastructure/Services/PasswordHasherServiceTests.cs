using FluentAssertions;
using MeuBarbeiro.Infrastructure.Security.Password;

namespace MeuBarbeiro.UnitTests.Infrastructure.Services;

public class PasswordHasherServiceTests
{
    private readonly PasswordHasherService _passwordHasherService = new();

    [Fact]
    public void Hash_DeveRetornarValorDiferenteDaSenhaOriginal_QuandoChamado()
    {
        // Arrange
        const string senha = "Senha@123";

        // Act
        var senhaHash = _passwordHasherService.Hash(senha);

        // Assert
        senhaHash.Should().NotBe(senha);
        senhaHash.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Veriry_DeveRetornarTrue_QuandoSenhaForCorreta()
    {
        // Arrange
        const string senha = "Senha@123";
        var senhaHash = _passwordHasherService.Hash(senha);

        // Act
        var result = _passwordHasherService.VerifyPasswordHash(senha, senhaHash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_DeveRetornarFalse_QuandoSenhaForIncorreta()
    {
        // Arrange 
        const string senhaCorreta = "Senha@123";
        const string senhaIncorreta = "Senha@321";

        var senhaHash = _passwordHasherService.Hash(senhaCorreta);

        // Act
        var result = _passwordHasherService.VerifyPasswordHash(senhaIncorreta, senhaHash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Hash_DeveGerarHashesDiferentes_QuandoExecutadoDuasVezes()
    {
        // Arrange
        const string senha = "Senha@123";

        // Act
        var primeiraSenhaHash = _passwordHasherService.Hash(senha);
        var segundaSenhaHash = _passwordHasherService.Hash(senha);

        // Assert
        primeiraSenhaHash.Should().NotBe(segundaSenhaHash);
    }

    [Fact]
    public void Verify_DeveValidarMesmaSenha_QuandoHashesForemDiferentes()
    {
        // Arrange
        const string senha = "Senha@123";

        var primeiraSenhaHash = _passwordHasherService.Hash(senha);
        var segundaSenhaHash = _passwordHasherService.Hash(senha);

        // Act 
        var resultPrimeiraHash = _passwordHasherService.VerifyPasswordHash(senha, primeiraSenhaHash);
        var resultSegundaHash = _passwordHasherService.VerifyPasswordHash(senha, segundaSenhaHash);

        // Assert
        resultPrimeiraHash.Should().BeTrue();
        resultSegundaHash.Should().BeTrue();
    }
}
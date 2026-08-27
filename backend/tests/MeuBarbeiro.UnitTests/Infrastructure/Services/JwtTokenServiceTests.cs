using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using MeuBarbeiro.Domain.Enums;
using MeuBarbeiro.Infrastructure.Security.Jwt;
using MeuBarbeiro.UnitTests.TestBuilder;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MeuBarbeiro.UnitTests.Infrastructure.Services;

public class JwtTokenServiceTests
{
    private const string Issuer = "MeuBarbeiro.Api";
    private const string Audience = "MeuBarbeiro.App";
    private const string SecretKey = "chave-ficticia-exclusiva-para-testes-com-pelo-menos-32-bytes";
    private const int ExpirationMinutes = 120;
    private readonly JwtTokenService _jwtTokenService;

    public JwtTokenServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = SecretKey,
                ["Jwt:Issuer"] = Issuer,
                ["Jwt:Audience"] = Audience,
                ["Jwt:ExpirationMinutes"] = ExpirationMinutes.ToString()
            })
            .Build();
        _jwtTokenService = new JwtTokenService(configuration);
    }

    [Fact]
    public void GenerateToken_DeveRetornarTokenNaoVazio_QuandoUsuarioForValido()
    {
        // Arrange
        var user = new UserBuilder()
            .WithName("Matheus")
            .WithEmail("matheus@email.com")
            .WithPasswordHash("senha-hash")
            .WithRole(UserRole.Client)
            .Build();

        // Act
        var token = _jwtTokenService.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateToken_DeveGerarTokenJwtValido_QuandoUsuarioForValido()
    {
        // Arrange
        var user = new UserBuilder()
            .WithName("Matheus")
            .WithEmail("matheus@email.com")
            .WithPasswordHash("senha-hash")
            .WithRole(UserRole.Client)
            .Build();

        var tokenHandler = new JwtSecurityTokenHandler();

        // Act
        var token = _jwtTokenService.GenerateToken(user);

        // Assert
        tokenHandler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public void GenerateToken_DeveConterUserIdNaClaimSub_QuandoTokenForGerado()
    {
        // Arrange
        var user = new UserBuilder()
            .WithName("Matheus")
            .WithEmail("matheus@email.com")
            .WithPasswordHash("senha-hash")
            .WithRole(UserRole.Client)
            .Build();

        // Act
        var token = _jwtTokenService.GenerateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        var subClaim = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
        subClaim.Should().Be(user.Id.ToString());
    }

    [Fact(Skip = "Finalizar quando mudar a tecnica de JWT")]
    public void GenerateToken_DeveConterUserIdNaClaimNameIdentifier_QuandoTokenForGerado()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact(Skip = "Finalizar quando mudar a tecnica de JWT")]
    public void GenerateToken_DeveConterNomeDoUsuario_QuandoTokenForGerado()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void GenerateToken_DeveConterEmailDoUsuario_QuandoTokenForGerado()
    {
        // Arrange
        var user = new UserBuilder()
            .WithName("Matheus")
            .WithEmail("matheus@email.com")
            .WithPasswordHash("senha-hash")
            .WithRole(UserRole.Client)
            .Build();

        // Act
        var token = _jwtTokenService.GenerateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        var emailClaim = jwt.Claims.First(c => c.Type == ClaimTypes.Email);
        emailClaim.Value.Should().Be(user.Email);
    }

    [Theory]
    [InlineData(UserRole.Client)]
    [InlineData(UserRole.Barber)]
    public void GenerateToken_DeveConterRoleDoUsuario_QuandoTokenForGerado(UserRole role)
    {
        // Arrange
        var user = new UserBuilder()
            .WithName("Matheus")
            .WithEmail("matheus@email.com")
            .WithPasswordHash("senha-hash")
            .WithRole(UserRole.Client)
            .Build();

        // Act
        var token = _jwtTokenService.GenerateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        var roleClaim = jwt.Claims.First(c => c.Type == ClaimTypes.Role);
        roleClaim.Value.Should().Be(user.Role.ToString());
    }

    [Fact]
    public void GenerateToken_DeveConterIssuerConfigurado_QuandoTokenForGerado()
    {
        // Arrange
        var user = new UserBuilder()
            .WithName("Matheus")
            .WithEmail("matheus@email.com")
            .WithPasswordHash("senha-hash")
            .WithRole(UserRole.Client)
            .Build();

        // Act
        var token = _jwtTokenService.GenerateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.Issuer.Should().Be(Issuer);
    }

    [Fact]
    public void GenerateToken_DeveConterAudienceConfigurada_QuandoTokenForGerado()
    {
        // Arrange
        var user = new UserBuilder()
            .WithName("Matheus")
            .WithEmail("matheus@email.com")
            .WithPasswordHash("senha-hash")
            .WithRole(UserRole.Client)
            .Build();

        // Act
        var token = _jwtTokenService.GenerateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.Audiences.First().Should().Be(Audience);
    }

    [Fact(Skip = "Finalizar quando mudar a tecnica de JWT")]
    public void GenerateToken_DeveDefinirExpiracaoFutura_QuandoTokenForGerado()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact(Skip = "Finalizar quando mudar a tecnica de JWT")]
    public void GenerateToken_DeveRespeitarTempoDeExpiracaoConfigurado_QuandoTokenForGerado()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact(Skip = "Finalizar quando mudar a tecnica de JWT")]
    public void GenerateToken_DeveAssinarTokenComHmacSha256_QuandoTokenForGerado()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void GenerateToken_DeveGerarTokenValidavelComAChaveConfigurada()
    {
        // Arrange
        var user = new UserBuilder()
            .WithName("Matheus")
            .WithEmail("matheus@email.com")
            .WithPasswordHash("senha-hash")
            .WithRole(UserRole.Client)
            .Build();

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)),

            ValidateIssuer = true,
            ValidIssuer = Issuer,

            ValidateAudience = true,
            ValidAudience = Audience,

            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero
        };

        // Act
        var token = _jwtTokenService.GenerateToken(user);
        var act = () => tokenHandler.ValidateToken(token, validationParameters, out _);

        // Assert
        act.Should().NotThrow();
    }

    [Fact(Skip = "Finalizar quando mudar a tecnica de JWT")]
    public void GenerateToken_DeveFalhar_QuandoSecaoJwtNaoExistir()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact(Skip = "Finalizar quando mudar a tecnica de JWT")]
    public void GenerateToken_DeveFalhar_QuandoSecretKeyForNulaOuVazia()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact(Skip = "Finalizar quando mudar a tecnica de JWT")]
    public void GenerateToken_DeveFalhar_QuandoSecretKeyForCurtaDemais()
    {
        // Arrange

        // Act

        // Assert
    }
}
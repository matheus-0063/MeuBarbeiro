using FluentAssertions;
using FluentValidation.Results;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Application.DTOs.Auth;
using MeuBarbeiro.Application.DTOs.Barbers;
using MeuBarbeiro.Application.DTOs.Clients;
using MeuBarbeiro.Application.Exceptions;
using MeuBarbeiro.Application.Services;
using MeuBarbeiro.Domain.Entities;
using MeuBarbeiro.UnitTests.TestBuilder;
using Microsoft.Extensions.Configuration;
using Moq;

namespace MeuBarbeiro.UnitTests.Application.Services;

public class AuthServiceTests
{
    private const string Issuer = "MeuBarbeiro.Api";
    private const string Audience = "MeuBarbeiro.App";
    private const string SecretKey = "chave-ficticia-exclusiva-para-testes-com-pelo-menos-32-bytes";
    private const int ExpirationMinutes = 120;

    private readonly IAuthService _authService;
    private readonly Mock<IBarberRepository> _barberRepositoryMock;
    private readonly Mock<IClientRepository> _clientRepositoryMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;

    private readonly Mock<IPasswordHasherService> _passwordHasherMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;

    private Func<Task<AuthResponse>>? _authFunc;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _clientRepositoryMock = new Mock<IClientRepository>();
        _barberRepositoryMock = new Mock<IBarberRepository>();

        _passwordHasherMock = new Mock<IPasswordHasherService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();

        var jwtConfiguration = new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = SecretKey,
            ["Jwt:Issuer"] = Issuer,
            ["Jwt:Audience"] = Audience,
            ["Jwt:ExpirationInMinutes"] = ExpirationMinutes.ToString()
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(jwtConfiguration)
            .Build();

        _authService = new AuthService(_userRepositoryMock.Object, _clientRepositoryMock.Object,
            _barberRepositoryMock.Object,
            _passwordHasherMock.Object, _jwtTokenServiceMock.Object);
    }

    [Fact]
    public async Task RegisterClientAsync_DeveCadastrarClienteERetornarToken_QuandoEmailNaoEstiverCadastrado()
    {
        // Arrange
        var request = CreateRegisterClientRequest();

        User? userAdded = null;
        Client? clientAdded = null;

        const string expectedToken = "token";

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback((User user, CancellationToken _) => userAdded = user);

        _clientRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
            .Callback((Client client, CancellationToken _) => clientAdded = client);

        _jwtTokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(expectedToken);

        // Act
        var result = await _authService.RegisterClientAsync(request);

        // Assert
        userAdded.Should().NotBeNull();
        clientAdded.Should().NotBeNull();

        result.AccessToken.Should().Be(expectedToken);

        result.UserId.Should().Be(userAdded.Id);
        result.Name.Should().Be(userAdded.Name);
        result.Email.Should().Be(userAdded.Email);
        result.Role.Should().Be(userAdded.Role.ToString());

        clientAdded.UserId.Should().Be(userAdded.Id);
    }

    [Fact]
    public async Task RegisterClientAsync_DeveLancarExcecao_QuandoEmailJaEstiverCadastrado()
    {
        // Arrange        
        var request = CreateRegisterClientRequest();

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User());

        // Act
        _authFunc = () => _authService.RegisterClientAsync(request);

        // Assert
        var exception = await Assert.ThrowsAsync<EmailAlreadyRegisteredException>(_authFunc);

        exception.Should().NotBeNull();
        exception.Message.Should().Be($"Email {request.Email} ja cadastrado.");

        _userRepositoryMock.Verify(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _clientRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterBarberAsync_DeveCadastrarBarbeiroERetornarToken_QuandoEmailNaoEstiverCadastrado()
    {
        // Arrange
        var request = CreateRegisterBarberRequest();

        User? userAdded = null;
        Barber? barberAdded = null;

        const string expectedToken = "token";

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback((User user, CancellationToken _) => userAdded = user);

        _barberRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Barber>(), It.IsAny<CancellationToken>()))
            .Callback((Barber barber, CancellationToken _) => barberAdded = barber);

        _jwtTokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(expectedToken);

        // Act
        var result = await _authService.RegisterBarberAsync(request);

        // Assert
        userAdded.Should().NotBeNull();
        barberAdded.Should().NotBeNull();

        result.AccessToken.Should().Be(expectedToken);

        result.UserId.Should().Be(userAdded.Id);
        result.Name.Should().Be(userAdded.Name);
        result.Email.Should().Be(userAdded.Email);
        result.Role.Should().Be(userAdded.Role.ToString());

        barberAdded.UserId.Should().Be(userAdded.Id);

        _userRepositoryMock.Verify(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _barberRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Barber>(), It.IsAny<CancellationToken>()), Times.Once);

        _jwtTokenServiceMock.Verify(x => x.GenerateToken(It.Is<User>(user => user.Id == userAdded.Id)), Times.Once);
    }

    [Fact]
    public async Task RegisterBarberAsync_DeveLancarExcecao_QuandoEmailJaEstiverCadastrado()
    {
        // Arrange
        var request = CreateRegisterBarberRequest();

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User());

        // Act
        _authFunc = () => _authService.RegisterBarberAsync(request);

        // Assert
        var exception = await Assert.ThrowsAsync<EmailAlreadyRegisteredException>(_authFunc);

        exception.Should().NotBeNull();
        exception.Message.Should().Be($"Email {request.Email} ja cadastrado.");

        _userRepositoryMock.Verify(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _barberRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Barber>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_DeveRetornarTokenEDadosDoUsuario_QuandoCredenciaisForemValidas()
    {
        // Arrange
        var request = CreateLoginRequest();

        var user = new UserBuilder()
            .WithEmail(request.Email)
            .WithPasswordHash("hash-armazenado")
            .Build();

        const string expectedToken = "token-gerado";

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.VerifyPasswordHash(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _jwtTokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(expectedToken);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.AccessToken.Should().Be(expectedToken);
        result.UserId.Should().Be(user.Id);
        result.Name.Should().Be(user.Name);
        result.Email.Should().Be(user.Email);
        result.Role.Should().Be(user.Role.ToString());

        _passwordHasherMock.Verify(x => x.VerifyPasswordHash(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _jwtTokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_DeveLancarExcecao_QuandoEmailNaoEstiverCadastrado()
    {
        // Arrange
        var request = CreateLoginRequest();

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        //Act
        var action = () => _authService.LoginAsync(request);

        //Assert
        await action.Should()
            .ThrowAsync<InvalidCredentialsException>()
            .WithMessage("E-mail ou senha inválidos.");

        _userRepositoryMock.Verify(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPasswordHash(request.Email, It.IsAny<string>()), Times.Never);
        _jwtTokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_DeveLancarExcecao_QuandoSenhaForInvalida()
    {
        // Arrange
        var request = CreateLoginRequest();

        var user = new UserBuilder()
            .WithEmail(request.Email)
            .Build();

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.VerifyPasswordHash(request.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var action = () => _authService.LoginAsync(request);

        // Assert
        await action.Should()
            .ThrowAsync<InvalidCredentialsException>()
            .WithMessage("E-mail ou senha inválidos.");

        _userRepositoryMock.Verify(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPasswordHash(request.Password, user.PasswordHash), Times.Once);
        _jwtTokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    #region Private Methods

    private static RegisterClientRequest CreateRegisterClientRequest()
    {
        return new RegisterClientRequest
        {
            Email = "matheus@gmail.com",
            Password = "123456",
            Name = "Matheus"
        };
    }

    private static RegisterBarberRequest CreateRegisterBarberRequest()
    {
        return new RegisterBarberRequest
        {
            Email = "matheus@gmail.com",
            Password = "123456",
            Name = "Matheus"
        };
    }

    private static LoginRequest CreateLoginRequest()
    {
        return new LoginRequest
        {
            Email = "matheus@gmail.com",
            Password = "123456"
        };
    }

    #endregion
}
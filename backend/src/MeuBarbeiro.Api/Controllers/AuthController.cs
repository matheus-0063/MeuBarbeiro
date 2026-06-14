using Asp.Versioning;
using MeuBarbeiro.Application.DTOs.Auth;
using MeuBarbeiro.Application.DTOs.Barbers;
using MeuBarbeiro.Application.DTOs.Clients;
using MeuBarbeiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuBarbeiro.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController(AuthService authService) : BaseController
{
    private readonly AuthService _authService = authService;

    [AllowAnonymous]
    [HttpPost("register/client")]
    public async Task<IActionResult> RegisterClient(RegisterClientRequest request)
    {
        var result = await _authService.RegisterClientAsync(request);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("register/barber")]
    public async Task<IActionResult> RegisterBarber(RegisterBarberRequest request)
    {
        var result = await _authService.RegisterBarberAsync(request);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }
}

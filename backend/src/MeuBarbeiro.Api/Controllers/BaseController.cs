using System.Security.Claims;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace MeuBarbeiro.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    private const string DefaultErrorMessages = "ErrorMessages";

    protected bool ResponseHasErros(ValidationResult result)
    {
        if (result.IsValid) return false;

        foreach (var error in result.Errors)
        {
            var fieldName = string.IsNullOrWhiteSpace(error.PropertyName)
                ? DefaultErrorMessages
                : error.PropertyName;

            ModelState.AddModelError(fieldName, error.ErrorMessage);
        }

        return true;
    }

    protected void AddProcessError(string? errorMessage, string? propertyName = null)
    {
        ModelState.AddModelError(propertyName ?? DefaultErrorMessages,
            errorMessage ?? "Houve um erro inesperado. Contate o suporte!");
    }

    protected void AddProcessError(IEnumerable<string> errorMessage, string? propertyName = null)
    {
        foreach (var error in errorMessage) AddProcessError(error, propertyName);
    }

    protected bool TryGetAuthenticatedUserId(out Guid userId)
    {
        userId = Guid.Empty;

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdValue, out userId);
    }
}
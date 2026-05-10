using FluentValidation.Results;

namespace MeuBarbeiro.Application.DTOs.Shared;

public class ServiceResult<T>
{
    public ServiceResult() {}

    public ServiceResult(T data, ValidationResult? validationResult = null)
    {
        Data = data;
        ValidationResult = validationResult ?? new ValidationResult();
    }

    public T? Data { get; set; }
    public ValidationResult ValidationResult { get; set; } = new ValidationResult();
    public bool IsNotFound { get; set; }
    public bool IsValid => ValidationResult.IsValid;
    
    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T>
        {
            Data = data,
            ValidationResult = new ValidationResult()
        };
    }

    public static ServiceResult<T> Failure(ValidationResult validationResult)
    {
        return new ServiceResult<T>
        {
            ValidationResult = validationResult
        };
    }

    public static ServiceResult<T> NotFound()
    {
        return new ServiceResult<T>
        {
            IsNotFound = true,
            ValidationResult = new ValidationResult()
        };
    }
}

using FluentValidation.Results;

namespace MeuBarbeiro.Application.DTOs.Shared;

public class ServiceResult
{
    public ValidationResult ValidationResult { get; protected init; } = new ValidationResult();
    public bool IsNotFound { get; protected init; }
    public bool IsForbidden { get; protected init; }
    public bool IsValid => ValidationResult.IsValid;
    public bool IsSuccess { get; protected init; }

    protected ServiceResult()
    {
    }

    public static ServiceResult NotFound()
    {
        return new ServiceResult { IsNotFound = true };
    }

    public static ServiceResult Forbidden()
    {
        return new ServiceResult { IsForbidden = true };
    }
    
    public static ServiceResult Success()
    {
        return new ServiceResult { IsSuccess = true };
    }

    public static ServiceResult Failure(ValidationResult validationResult)
    {
        return new ServiceResult { ValidationResult = validationResult };
    }
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; private set; }

    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T>
        {
            Data = data,
            IsSuccess = true
        };
    }

    public new static ServiceResult<T> Forbidden()
    {
        return new ServiceResult<T>
        {
            IsForbidden = true
        };
    }

    public new static ServiceResult<T> Failure(ValidationResult validationResult)
    {
        return new ServiceResult<T>
        {
            ValidationResult = validationResult,
        };
    }

    public new static ServiceResult<T> NotFound()
    {
        return new ServiceResult<T>
        {
            IsNotFound = true
        };
    }
}
namespace MeuBarbeiro.Application.Exceptions;

public sealed class EmailAlreadyRegisteredException(string email) 
    : Exception($"Email {email} ja cadastrado.");
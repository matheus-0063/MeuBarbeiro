using MeuBarbeiro.Domain.Entities;
using MeuBarbeiro.Domain.Enums;

namespace MeuBarbeiro.UnitTests.TestBuilder;

public class UserBuilder
{
    private string _name = string.Empty;
    private string _email = string.Empty;
    private string _passwordHash = string.Empty;
    private UserRole _role = default;
    private DateTime _createdAt = DateTime.Now;

    public UserBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithPasswordHash(string passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    public UserBuilder WithRole(UserRole role)
    {
        _role = role;
        return this;
    }

    public User Build()
    {
        var user = new User(
            name: _name,
            email: _email,
            passwordHash: _passwordHash,
            role: _role);
        
        return user;
    }
}
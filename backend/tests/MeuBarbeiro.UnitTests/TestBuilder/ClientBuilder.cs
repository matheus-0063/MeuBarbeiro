using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.UnitTests.TestBuilder;

public class ClientBuilder
{
    private Guid _userId = Guid.NewGuid();

    public ClientBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public Client Build()
    {
        var client = new Client(
            userId: _userId
        );
        
        return client;
    }
}
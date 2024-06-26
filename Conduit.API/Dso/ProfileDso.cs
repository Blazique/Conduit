using Marten.Schema;

namespace Conduit.API.Dso;

public record ProfileDso
{
    public required string Id { get; init; }

    [Identity]
    public required string Username { get; init; }
    public required string Bio { get; init; }
    public required string Image { get; init; }
    public bool Following { get; init; }

}
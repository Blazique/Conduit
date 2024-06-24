namespace Conduit.API.Dso;

public record ProfileDso
{
    [Marten.Schema.Identity]
    public required string Username { get; init; }
    public required string Bio { get; init; }
    public required string Image { get; init; }
    public bool Following { get; init; }

}
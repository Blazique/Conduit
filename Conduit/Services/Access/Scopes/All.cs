namespace Conduit.Services.Access.Scopes;

public record All : Scope<Frontend>
{
    public static string Name => $"{Frontend.Name}:all";

    public static string Description => "Gives access to all scopes";
}

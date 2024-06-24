namespace Conduit;

public record IdentityDatabase : Database, Component<IdentityDatabase>
{
    public static string Name => "identity";

    public static string Description => throw new NotImplementedException();
}

namespace Conduit;

public record IdentityDatabase : ConduitDatabase, Component<IdentityDatabase>
{
    public static string Name => "identity";

    public static string Description => throw new NotImplementedException();
}

namespace Conduit;

public record ConduitDatabase : Database, Component<ConduitDatabase>
{
    public static string Name => "conduit";

    public static string Description => throw new NotImplementedException();
}

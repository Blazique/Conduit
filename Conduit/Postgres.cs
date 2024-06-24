using Radix.Generators;

namespace Conduit;

public record Postgres : Component<Postgres>
{
    public static string Name => "postgres";

    public static string Description => throw new NotImplementedException();
}

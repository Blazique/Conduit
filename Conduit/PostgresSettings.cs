using Radix.Generators.Attributes;

namespace Conduit;

[Configuration]
public record PostgresSettings
{
    public required string ConnectionString { get; init; }
}

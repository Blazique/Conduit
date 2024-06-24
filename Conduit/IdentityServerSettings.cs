using Radix.Generators.Attributes;

namespace Conduit;

[Configuration]
public record IdentityServerSettings
{
    public string Authority { get; init; } = default!;
}
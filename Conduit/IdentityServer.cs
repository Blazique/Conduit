using System.Net;
using Conduit.Services.Access;
using Microsoft.Extensions.Configuration;

namespace Conduit;

/// <summary>
/// The identity server service provides authentication and authorization services.
/// </summary>
public record IdentityServer : Component<IdentityServer>
{
    public static string Name => "identityserver";

    public static string Description => throw new NotImplementedException();
}

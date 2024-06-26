
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ServiceDiscovery;
using Microsoft.Win32;

namespace Conduit.IdentityServer;

public static class Configuration
{
    public static IEnumerable<ApiScope> ApiScopes =>
    [
        new ApiScope(name: Services.Access.Scopes.All.Name, displayName: Services.Access.Scopes.All.Description) 
    ];


    public static IEnumerable<Client> Clients =>
    [

        new Client
        {
            ClientId = Frontend.Name,

            // no interactive user, use the clientid/secret for authentication
            AllowedGrantTypes = GrantTypes.Code,

            // secret for authentication
            ClientSecrets =
            {
                new Secret("secret".Sha256())
            },

            // where to redirect to after login 
            // todo get this from configuration
            RedirectUris = { $"https://localhost:7199/signin-oidc" },

            // where to redirect to after logout
            PostLogoutRedirectUris = { $"https://localhost:7199/signout-callback-oidc" },

            // scopes that client has access to
            AllowedScopes =
            [
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                IdentityServerConstants.StandardScopes.OfflineAccess,
                Services.Access.Scopes.All.Name,
                "verification"
            ],

            AllowOfflineAccess = true
        }
    ];

    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResource()
        {
            Name = "verification",
            UserClaims = new List<string>
            {
                JwtClaimTypes.Email,
                JwtClaimTypes.EmailVerified
            }
        }
    ];
}
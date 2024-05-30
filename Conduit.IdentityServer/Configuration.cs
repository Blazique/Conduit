﻿
using Duende.IdentityServer.Models;

namespace Conduit.IdentityServer;

public record Scope(string Name, string DisplayName);

public static class Scopes
{
    public static readonly Scope Full = new("conduit.full", "Conduit.API.Full");
}

public static class Configuration
{
    public static IEnumerable<ApiScope> ApiScopes =>
    [
        new ApiScope(name: Scopes.Full.Name, displayName: Scopes.Full.DisplayName) 
    ];

    public static IEnumerable<Client> Clients =>
    [
        new Client
        {
            ClientId = "conduit",

            // no interactive user, use the clientid/secret for authentication
            AllowedGrantTypes = GrantTypes.Code,

            // secret for authentication
            ClientSecrets =
            {
                new Secret("secret".Sha256())
            },

            // where to redirect to after login
            RedirectUris = { "https://localhost:7199/signin-oidc" },

            // where to redirect to after logout
            PostLogoutRedirectUris = { "https://localhost:7199/signout-callback-oidc" },

            // scopes that client has access to
            AllowedScopes = 
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                Scopes.Full.Name 
            }
        }
    ];

    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
    ];
}
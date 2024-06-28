
using Conduit.Domain;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Radix.Data;
using System.Security.Claims;

namespace Conduit.Components;

[InteractiveServerRenderMode(Prerender = false)]
public class NavBar : Blazique.Web.Component
{
    private ClaimsPrincipal? _user;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthState { get; set; }

    public override Node[] Render()
    =>
    [
        // If the user is not logged in, then display the NavBarForUsersThatAreNotLoggedIn
        _user is not null && _user.Identity.IsAuthenticated
        ? NavBarForLoggedInUsers(_user)
        : NavBarForUsersThatAreNotLoggedIn()

    ];

    private IDisposable? _subscription;

    protected override async Task OnInitializedAsync()
    {
        if (AuthState == null)
        {
            return;
        }

        var authState = await AuthState;
        _user = authState.User;
    }

    public void Dispose()
    {
        _subscription?.Dispose();
    }

    private static Node NavBarForUsersThatAreNotLoggedIn()
    {
        return
           
            nav([@class(["navbar", "navbar-light"])], [
                div([@class(["container"])], [
                    a([@class(["navbar-brand"]), href(["/"])], [text("conduit")]),
                    ul([@class(["nav", "navbar-nav", "pull-xs-right"])], [
                        li([@class(["nav-item"])], [
                            navLink(NavLinkMatch.All,[@class(["nav-link"]), href(["/"])], [text("Home")])
                        ]),
                        li([@class(["nav-item"])], [
                            navLink(NavLinkMatch.All,[@class(["nav-link"]), href(["/login"])], [text("Sign in")])
                        ]),
                        li([@class(["nav-item"])], [
                            navLink(NavLinkMatch.All,[@class(["nav-link"]), href(["/register"])], [text("Sign up")])
                        ])
                    ])
                ])
            ]);;
    }

    private static Node NavBarForLoggedInUsers(ClaimsPrincipal user)
    {
        // Retrieve the "name" claim value
        var nameClaimValue = user.Claims.Where(claim => claim.Type == "name").FirstOrDefault().Value ?? "Unknown";
        var userNameClaimValue = user.Claims.Where(claim => claim.Type == "given_name").FirstOrDefault().Value ?? "Unknown";

        return nav([@class(["navbar", "navbar-light"])], [
                    div([@class(["container"])], [
                     a([@class(["navbar-brand"]), href(["/"])], [text("conduit")]),
                     ul([@class(["nav", "navbar-nav", "pull-xs-right"])], [
                         li([@class(["nav-item"])], [
                             navLink(NavLinkMatch.All, [@class(["nav-link"]), href(["/"])], [text("Home")])
                         ]),
                         li([@class(["nav-item"])], [
                             navLink(NavLinkMatch.All, [@class(["nav-link"]), href(["/editor"])], [
                                 i([@class(["ion-compose"])], []),
                                 text(" New Article")
                             ])
                         ]),
                         li([@class(["nav-item"])], [
                             navLink(NavLinkMatch.All, [@class(["nav-link"]), href(["/settings"])], [
                                 i([@class(["ion-gear-a"])], []),
                                 text(" Settings")
                             ])
                         ]),
                        li([@class(["nav-item"])], [
                            a([@class(["nav-link"]), href([$"/profile/{userNameClaimValue}"])], [text(nameClaimValue)])
                        ])
                     ])
                 ])
                ]);
    }
}


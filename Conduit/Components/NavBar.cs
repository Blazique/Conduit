namespace Conduit.Components;

using System;
using Conduit.Domain;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Radix.Data;


[InteractiveServerRenderMode(Prerender = false)]
public class NavBar : Blazique.Web.Component
{
    [Inject]
    public MessageBus MessageBus { get; set; } = null!;

    [Inject]
    public GetUser GetUser { get; set; } = null!;

    private User? _user;

    private bool isUserDataRetrieved = false;

    public override Node[] Render()
    =>
    [
        // If the user is not logged in, then display the NavBarForUsersThatAreNotLoggedIn
        _user switch
        {
            // If the user is logged in, then display the NavBarForLoggedInUsers
            User user => NavBarForLoggedInUsers(user),
            _ => NavBarForUsersThatAreNotLoggedIn()
        }             
    ];

    private IDisposable? _subscription;

    protected override void OnInitialized()
    {
        _subscription = MessageBus.OfType<UserLoggedIn>().Subscribe(HandleMessage);
    }

    private void HandleMessage(UserLoggedIn message)
    {
        _user = message.User;
        StateHasChanged();
    }

    public void Dispose()
    {
        _subscription?.Dispose();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // If the user is not logged in and the user data has not been retrieved, then retrieve the user data
        if(_user is null && !isUserDataRetrieved) 
        {  
            var userOption = await GetUser();
            switch (userOption)
            {
                case Some<User>(var user):
                    _user = user;
                    break;
                case None<User> _:
                    _user = null;
                    break;
            }
            isUserDataRetrieved = true;
            StateHasChanged();
        }        
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

    private static Node NavBarForLoggedInUsers(User user)
    {
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
                            a([@class(["nav-link"]), href([$"/profile/{user.Username}"])], [text(user.Username)])
                        ])
                     ])
                 ])
                ]);
    }
}


using Microsoft.AspNetCore.Components.Routing;

namespace Conduit.Components.Layout;

public class MainLayout : Blazique.Web.Component
{
    [Parameter]
    public RenderFragment? Body { get; set; }

    public override Node[] Render()
     =>
     [
        Navbar(),
        Body is not null ? fragment(Body) : empty(),
        footer([@class(["footer"])], [
            div([@class(["container"])], [
                a([@class(["logo-font"]), href(["/"])], [text("conduit")]),
                span([@class(["attribution"])], [
                    text("An interactive learning project from "),
                    a([href(["https://thinkster.io"]), target(["_blank"])], [text("Thinkster")]),
                    text(". Code & design licensed under MIT.")
                ])])])
    ];

    public Node Navbar() =>
        IsUserLoggedIn()
            ? NavBarForLoggedInUsers()
            : NavBarForUsersThatAreNotLoggedIn();

    private bool IsUserLoggedIn()
    {
        return false;
    }

    private static Node NavBarForLoggedInUsers()
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
                            a([@class(["nav-link"]), href(["/profile/username"])], [text("username")])
                        ])
                     ])
                 ])
                ]);
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
            ]);
    }
}

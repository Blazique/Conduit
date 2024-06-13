using Conduit.Domain;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Radix.Data;
using static Radix.Control.Option.Extensions;
using static Blazique.Attribute;

namespace Conduit.Components.Layout;

public class MainLayout : Blazique.Web.Component
{
    [Parameter]
    public RenderFragment? Body { get; set; }

    public override Node[] Render()
     =>
     [
        //component<AuthorizeView>([
        //    fragment<AuthenticationState>("Authorized", authenticationState => [
        //            component<NavBar>([], []),
        //         Body is not null ? fragment(Body) : empty(),
        //        ]),
        //    fragment<AuthenticationState>("NotAuthorized", authenticationState => [
                    
        //         text("Unauthorized")
        //        ])
        //    ],[]),
        component<NavBar>([], []),
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

    

    
}

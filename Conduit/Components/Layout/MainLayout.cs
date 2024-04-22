using Conduit.Domain;
using Microsoft.AspNetCore.Components.Routing;
using Radix.Data;
using static Radix.Control.Option.Extensions;

namespace Conduit.Components.Layout;

public class MainLayout : Blazique.Web.Component
{
    [Parameter]
    public RenderFragment? Body { get; set; }

    public override Node[] Render()
     =>
     [
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

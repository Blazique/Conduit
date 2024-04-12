using Blazique;
using Radix;
using System.Runtime.CompilerServices;
using static Conduit.Components.Elements;
using static Conduit.Components.Attributes;

using Conduit.Components.Names.Elements;
using Microsoft.AspNetCore.Components.Web;

namespace Conduit.Components;


public class App : Blazique.Web.Component
{
    public override Node[] Render() =>
        [
            doctype([Attributes.html([])], []),
            html([lang(["en"])], []),
            head([], [
                meta([charset(["utf-8"])], []),
                meta([name(["viewport"]), content(["width=device-width, initial-scale=1.0"])], []),
                @base([href(["/"])], []),
                link([rel(["stylesheet"]), href(["app.css"])], []),
                link([rel(["stylesheet"]), href(["Conduit.styles.css"])], []),
                link([rel(["icon"]), type(["image/png"]), href(["favicon.png"])], []),
                component<HeadOutlet>([],[]),
            ]),
            body([], [
                component<Routes>([], []),
                script([src(["_framework/blazor.web.js"])], []),
            ])
        ];

    

    
}

public static class Elements
{
    public static Node doctype(Blazique.Data.Attribute[] attributes, Node[] children, object? key = null, [CallerLineNumber] int nodeId = 0) =>
        Element.Create<doctype>(attributes, children, key, nodeId);

}

public static class Attributes
{
    public static Blazique.Data.Attribute html(string[] values, [CallerLineNumber] int nodeId = 0) =>
        Blazique.Attribute.Create<Conduit.Components.Names.Attributes.html>(values, nodeId);
}

using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components;
using Blazique;
using Radix;
using System.Runtime.CompilerServices;
using static Conduit.Components.Elements;
using static Conduit.Components.Attributes;

using Conduit.Components.Names.Elements;
using Microsoft.AspNetCore.Components.Web;

namespace Conduit.Components;

public class Routes : Blazique.Web.Component
{
    public override Node[] Render() =>
        [
            component<Router>([attribute("AppAssembly", [typeof(Program).Assembly]), 
                fragment<Microsoft.AspNetCore.Components.RouteData>("Found", routeData =>
                [
                    component<RouteView>([attribute("RouteData", [routeData]), attribute("DefaultLayout", [typeof(Layout.MainLayout)])], []),
                    component<FocusOnNavigate>([attribute("RouteData", [routeData]), attribute("Selector", ["h1"])], [])
                ])
            ],[])
            
        ];
        

    public static Blazique.Data.Attribute attribute(string name, object[] values, [CallerLineNumber] int nodeId = 0) =>
        Blazique.Attribute.Create(name, values, nodeId);

    public static Blazique.Data.Attribute fragment(string name, Node[] node, [CallerLineNumber] int nodeId = 0) => 
    (component, builder) =>
        builder.AddAttribute(nodeId, name, new RenderFragment(rt => {
            foreach (var n in node)
                n.Invoke(component, rt);
        }));

    public static Blazique.Data.Attribute fragment<T>(string name, Func<T, Node[]> f, [CallerLineNumber] int nodeId = 0) => 
    (component, builder) =>
        builder.AddAttribute(nodeId, name, new RenderFragment<T>(context => 
            new RenderFragment(rt => 
            {
                var node = f(context);
                foreach (var n in node)
                    n.Invoke(component, rt);
            })));
}

// < Router AppAssembly = "typeof(Program).Assembly" >
//    < Found Context = "routeData" >
//        < RouteView RouteData = "routeData" DefaultLayout = "typeof(Layout.MainLayout)" />
//        < FocusOnNavigate RouteData = "routeData" Selector = "h1" />
//    </ Found >
// </ Router >




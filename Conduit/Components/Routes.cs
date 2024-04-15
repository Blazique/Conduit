using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components;
using static Blazique.Attribute;

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
}

// The original blazor razor file content:

// < Router AppAssembly = "typeof(Program).Assembly" >
//    < Found Context = "routeData" >
//        < RouteView RouteData = "routeData" DefaultLayout = "typeof(Layout.MainLayout)" />
//        < FocusOnNavigate RouteData = "routeData" Selector = "h1" />
//    </ Found >
// </ Router >




using Microsoft.AspNetCore.Components.Web;

namespace Conduit.Components;


public class App : Blazique.Web.Component
{
    public override Node[] Render() =>
        [
            doctype([Attributes.html([])]
            ),
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

// The original blazor razor file content:

// <!DOCTYPE html>
// <html lang="en">

// <head>
//     <meta charset="utf-8" />
//     <meta name="viewport" content="width=device-width, initial-scale=1.0" />
//     <base href="/" />
//     <link rel="stylesheet" href="bootstrap/bootstrap.min.css" />
//     <link rel="stylesheet" href="app.css" />
//     <link rel="stylesheet" href="BlazorDefault.styles.css" />
//     <link rel="icon" type="image/png" href="favicon.png" />
//     <HeadOutlet />
// </head>

// <body>
//     <Routes />
//     <script src="_framework/blazor.web.js"></script>
// </body>

// </html>

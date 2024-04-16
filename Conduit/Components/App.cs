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
                title([], [text("Conduit")]),
                @base([href(["/"])], []),
                link([rel(["stylesheet"]), href(["//code.ionicframework.com/ionicons/2.0.1/css/ionicons.min.css"])], []),
                link([rel(["stylesheet"]), href(["//fonts.googleapis.com/css?family=Titillium+Web:700|Source+Serif+Pro:400,700|Merriweather+Sans:400,700|Source+Sans+Pro:400,300,600,700,300italic,400italic,600italic,700italic"])], []),
                link([rel(["stylesheet"]), href(["//demo.productionready.io/main.css"])], []),
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

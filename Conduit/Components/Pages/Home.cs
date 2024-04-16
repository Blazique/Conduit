namespace Conduit.Components;

[Route("/")]
public class Home : Blazique.Web.Component
{
    public override Node[] Render()
    =>
    [
        div([@class(["home-page"])], [
            div([@class(["banner"])], [
                div([@class(["container"])], [
                    h1([], [text("conduit")]),
                    p([], [text("A place to share your knowledge.")])
                ])])]),
        div([@class(["container page"])], [
            div([@class(["row"])], [
                div([@class(["col-md-9"])], [
                    div([@class(["feed-toggle"])], [
                        ul([@class(["nav", "nav-pills", "outline-active"])], [
                            li([@class(["nav-item"])], [
                                a([@class(["nav-link"]), href([""])], [text("Your Feed")])
                            ]),
                            li([@class(["nav-item"])], [
                                a([@class(["nav-link", "active"]), href([""])], [text("Global Feed")])
                            ])
                        ])
                    ]),
                    div([@class(["article-preview"])], [
                        div([@class(["article-meta"])], [
                            a([@class(["author"]), href(["/profile/eric-simons"])], [
                                img([src(["http://i.imgur.com/Qr71crq.jpg"])], [])]),
                            div([@class(["info"])], [
                                a([@class(["author"]), href(["/profile/eric-simons"])], [
                                    text("Eric Simons")])],
                                span([@class(["date"])], [text("January 20th")])),
                            button([@class(["btn", "btn-outline-primary", "btn-sm", "pull-xs-right"])], [
                                i([@class(["ion-heart"])], []),
                        ]),
                        a([href(["/article/how-to-build-webapps-that-scale"]), @class(["preview-link"])], [
                            h1([], [text("How to build webapps that scale")]),
                            p([], [text("This is the description for the post.")]),
                            span([], [text("Read more...")]),
                            ul([@class(["tag-list"])], [
                                li([@class(["tag-pill", "tag-default"])], [text("realworld")]),
                                li([@class(["tag-pill", "tag-default"])], [text("implementations")])
                            ])
                        ])
                    ])]),
                    div([@class(["article-preview"])], [
                        div([@class(["article-meta"])], [
                            a([@class(["author"]), href(["/profile/albert-pai"])], [
                                img([src(["http://i.imgur.com/N4VcUeJ.jpg"])], [])]),
                            div([@class(["info"])], [
                                a([@class(["author"]), href(["/profile/albert-pai"])], [
                                    text("Albert Pai")])],
                                span([@class(["date"])], [text("January 20th")])),
                            button([@class(["btn", "btn-outline-primary", "btn-sm", "pull-xs-right"])], [
                            i([@class(["ion-heart"])], [])])]),
                        a([href(["/article/the-song-you"]), @class(["preview-link"])], [
                            h1([], [text("The song you won't ever stop singing. No matter how hard you try.")]),
                            p([], [text("This is the description for the post.")]),
                            span([], [text("Read more...")]),
                            ul([@class(["tag-list"])], [
                                li([@class(["tag-pill", "tag-default"])], [text("realworld")]),
                                li([@class(["tag-pill", "tag-default"])], [text("implementations")])
                            ])
                        ])
                    ]),
                    ul([@class(["pagination"])], [
                        li([@class(["page-item"])], [
                            a([@class(["page-link"]), href([""])], [text("1")])]),
                        li([@class(["page-item"])], [
                            a([@class(["page-link"]), href([""])], [text("2")])])]),
                    ]),
                    div([@class(["col-md-3"])], [
                        div([@class(["sidebar"])], [
                            p([], [text("Popular Tags")]),
                            div([@class(["tag-list"])], [
                                a([@class(["tag-pill", "tag-default"]), href([""])], [text("programming")]),
                                a([@class(["tag-pill", "tag-default"]), href([""])], [text("javascript")]),
                                a([@class(["tag-pill", "tag-default"]), href([""])], [text("emberjs")]),
                                a([@class(["tag-pill", "tag-default"]), href([""])], [text("angularjs")]),
                                a([@class(["tag-pill", "tag-default"]), href([""])], [text("react")]),
                                a([@class(["tag-pill", "tag-default"]), href([""])], [text("mean")]),
                                a([@class(["tag-pill", "tag-default"]), href([""])], [text("node")]),
                                a([@class(["tag-pill", "tag-default"]), href([""])], [text("rails")])
                            ])
                        ])
                    ])
                ])
            ]),
    ];
}
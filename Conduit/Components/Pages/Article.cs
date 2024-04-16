namespace Conduit.Components;

[Route("/Article")]
public class Article : Blazique.Web.Component
{
    public override Node[] Render()
    =>
    [
        div([@class(["article-page"])], [
            div([@class(["banner"])], [
                div([@class(["container"])], [
                    h1([], [text("How to build webapps that scale")]),
                    div([@class(["article-meta"])], [
                        a([href(["/profile/eric-simons"])], [
                            img([src(["http://i.imgur.com/Qr71crq.jpg"])], [])
                        ]),
                        div([@class(["info"])], [
                            a([@class(["author"]), href(["/profile/eric-simons"])], [text("Eric Simons")]),
                            span([@class(["date"])], [text("January 20th")])
                        ]),
                        button([@class(["btn", "btn-sm", "btn-outline-secondary"]), type(["button"])], [
                            i([@class(["ion-plus-round"])], []),
                            text(" Follow Eric Simons"),
                            span([@class(["counter"])], [text("(10)")])
                        ]),
                        text("&nbsp;"),
                        text("&nbsp;"),
                        button([@class(["btn", "btn-sm", "btn-outline-primary"]), type(["button"])], [
                            i([@class(["ion-heart"])], []),
                            text(" 29")
                        ]),
                        button([@class(["btn", "btn-sm", "btn-outline-secondary"]), type(["button"])], [
                            i([@class(["ion-compose"])], []),
                            text(" Edit Article")
                        ]),
                        button([@class(["btn", "btn-sm", "btn-outline-danger"]), type(["button"])], [
                            i([@class(["ion-trash-a"])], []),
                            text(" Delete Article")
                        ])
                    ])
                ])
            ]),
            div([@class(["container", "page"])],[
                div([@class(["row", "article-content"])], [
                    div([@class(["col-md-12"])],[
                        p([], [text("Web development technologies have evolved at an incredible clip over the past few years.")]),
                        h2([id(["introducing-ionic"])], [text("Introducing RealWorld.")]),
                        p([], [text("It's a great solution for learning how other frameworks work.")]),
                        ul([@class(["tag-list"])], [
                            li([@class(["tag-default", "tag-pill", "tag-outline"])], [text("realworld")]),
                            li([@class(["tag-default", "tag-pill", "tag-outline"])], [text("implementations")])
                        ]),
                    ])
                ])
            ]),
            hr([], []),
            div([@class(["article-actions"])], [
                div([@class(["article-meta"])], [
                    a([href(["profile.html"])], [
                        img([src(["http://i.imgur.com/Qr71crq.jpg"])], [])
                    ]),
                    div([@class(["info"])], [
                        a([@class(["author"]), href([""])], [text("Eric Simons")]),
                        span([@class(["date"])], [text("January 20th")])
                    ]),
                    button([@class(["btn", "btn-sm", "btn-outline-secondary"]), type(["button"])], [
                        i([@class(["ion-plus-round"])], []),
                        text(" Follow Eric Simons")
                    ]),
                    button([@class(["btn", "btn-sm", "btn-outline-primary"]), type(["button"])], [
                        i([@class(["ion-heart"])], []),
                        text(" Favorite Article"),
                    ]),
                    button([@class(["btn", "btn-sm", "btn-outline-primary"]), type(["button"])], [
                        i([@class(["ion-heart"])], []),
                        span([@class(["counter"])], [text("29")]),
                    button([@class(["btn", "btn-sm", "btn-outline-secondary"]), type(["button"])], [
                        i([@class(["ion-edit"])], []),
                        text(" Edit Article")
                    ]),
                    button([@class(["btn", "btn-sm", "btn-outline-danger"]), type(["button"])], [
                        i([@class(["ion-trash-a"])], []),
                        text(" Delete Article")
                    ])
                ])
            ]),
            div([@class(["row"])], [
                div([@class(["col-xs-12", "col-md-8", "offset-md-2"])], [
                    form([@class(["card", "comment-form"])], [
                        div([@class(["card-block"])], [
                                textarea([@class(["form-control"]), rows(["3"]), placeholder(["Write a comment..."])], [])
                            ]),
                        div([@class(["card-footer"])], [
                            img([src(["http://i.imgur.com/Qr71crq.jpg"]), @class(["comment-author-img"])], []),
                            button([@class(["btn", "btn-sm", "btn-primary"]), type(["submit"])], [text("Post Comment")])
                        ])
                    ]),
                    div([@class(["card"])], [
                        div([@class(["card-block"])], [
                            p([@class(["card-text"])], [text("With supporting text below as a natural lead-in to additional content.")]),
                        ]),
                        div([@class(["card-footer"])], [
                            a([@class(["comment-author"]), href(["/profile/author"])], [
                                img([src(["http://i.imgur.com/Qr71crq.jpg"]), @class(["comment-author-img"])], [])
                            ]),
                            text("&nbsp;"),
                            a([@class(["comment-author"]), href(["/profile/jacob-schmidt"])], [text("Jacob Schmidt")]),
                            span([@class(["date-posted"])], [text("Dec 29th")]),
                        ])
                    ]),
                    div([@class(["card"])], [
                        div([@class(["card-block"])], [
                            p([@class(["card-text"])], [text("With supporting text below as a natural lead-in to additional content.")]),
                        ]),
                        div([@class(["card-footer"])], [
                            a([@class(["comment-author"]), href(["/profile/author"])], [
                                img([src(["http://i.imgur.com/Qr71crq.jpg"]), @class(["comment-author-img"])], [])
                            ]),
                            text("&nbsp;"),
                            a([@class(["comment-author"]), href(["/profile/jacob-schmidt"])], [text("Jacob Schmidt")]),
                            span([@class(["date-posted"])], [text("Dec 29th")]),
                            span([@class(["mod-options"])], [
                                i([@class(["ion-edit"])], []),
                                i([@class(["ion-trash-a"])], [])
                            ])
                        ])
                    ])
                ])
            ])
        ])
    ])
    ];


                    
    
}
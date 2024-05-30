namespace Conduit.Components;

[Route("/Editor")]
public class Editor : Blazique.Web.Component
{
    public override Node[] Render()
    =>
    [
        div([@class(["editor-page"])], [
            div([@class(["container", "page"])], [
                div([@class(["row"])], [
                    div([@class(["col-md-10", "offset-md-1", "col-xs-12"])], [
                        form([], [
                            fieldset([], [
                                fieldset([@class(["form-group"])], [
                                    input([@class(["form-control", "form-control-lg"]), type(["text"]), placeholder(["Article Title"])], [])
                                ]),
                                fieldset([@class(["form-group"])], [
                                    input([@class(["form-control", "form-control-lg"]), type(["text"]), placeholder(["What's this article about?"])], [])
                                ]),
                                fieldset([@class(["form-group"])], [
                                    textarea([@class(["form-control", "form-control-lg"]), rows(["8"]), placeholder(["Write your article (in markdown)"])], [])
                                ]),
                                fieldset([@class(["form-group"])], [
                                    input([@class(["form-control", "form-control-lg"]), type(["text"]), placeholder(["Enter tags"])], []),
                                    div([@class(["tag-list"])], [
                                        span([@class(["tag-default", "tag-pill"])], [
                                            i([@class(["ion-close-round"])], []),
                                            text(" tag ")
                                        ])
                                    ])
                                ]),
                                button([@class(["btn", "btn-lg", "pull-xs-right", "btn-primary"]), type(["button"])], [text("Publish Article")])
                            ])
                        ])
                    ])
                ])
            ])
        ])
    ];
}
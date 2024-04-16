namespace Conduit.Components;

[Route("/Login")]
public class Login : Blazique.Web.Component
{
    public override Node[] Render()
    =>
    [
        div([@class(["auth-page"])], [
            div([@class(["container", "page"])], [
                div([@class(["row"])], [
                    div([@class(["col-md-6", "offset-md-3", "col-xs-12"])], [
                        h1([@class(["text-xs-center"])], [text("Sign in")]),
                        p([@class(["text-xs-center"])], [
                            a([href(["/register"])], [text("Need an account?")])
                        ]),
                        ul([@class(["error-messages"])], [
                            li([], [text("That email is already taken")])
                        ]),
                        form([], [
                            fieldset([@class(["form-group"])], [
                                input([@class(["form-control", "form-control-lg"]), type(["text"]), placeholder(["Email"])], [])
                            ]),
                            fieldset([@class(["form-group"])], [
                                input([@class(["form-control", "form-control-lg"]), type(["password"]), placeholder(["Password"])], [])
                            ]),
                            button([@class(["btn", "btn-lg", "btn-primary", "pull-xs-right"]), type(["submit"])], [text("Sign in")])
                        ])
                    ])
                ])
            ])
        ])
    ];
}
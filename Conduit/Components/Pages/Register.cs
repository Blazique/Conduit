
namespace Conduit.Components;

[Route("/Register")]
public class Register : Component<RegisterModel, RegisterCommand>
{
    public override ValueTask<RegisterModel> Update(RegisterModel model, RegisterCommand command)
    {
        
    }

    public override Node[] View(RegisterModel model, Func<RegisterCommand, Task> dispatch) =>
    [
        div([@class(["auth-page"])], [
            div([@class(["container", "page"])], [
                div([@class(["row"])], [
                    div([@class(["col-md-6", "offset-md-3", "col-xs-12"])], [
                        h1([@class(["text-xs-center"])], [text("Sign up")]),
                        p([@class(["text-xs-center"])], [
                            a([href(["/login"])], [text("Have an account?")])
                        ]),
                        ul([@class(["error-messages"])], [
                            li([], [text("That email is already taken")])
                        ]),
                        form([], [
                            fieldset([@class(["form-group"])], [
                                input([@class(["form-control", "form-control-lg"]), type(["text"]), placeholder(["Username"])], [])
                            ]),
                            fieldset([@class(["form-group"])], [
                                input([@class(["form-control", "form-control-lg"]), type(["text"]), placeholder(["Email"])], [])
                            ]),
                            fieldset([@class(["form-group"])], [
                                input([@class(["form-control", "form-control-lg"]), type(["password"]), placeholder(["Password"])], [])
                            ]),
                            button([@class(["btn", "btn-lg", "btn-primary", "pull-xs-right"]), type(["submit"])], [text("Sign up")])
                        ])
                    ])
                ])
            ])
        ])
    ];
}

public record RegisterCommand(string Username, string Email, string Password);

public record RegisterModel(string Username, string Email, string Password);
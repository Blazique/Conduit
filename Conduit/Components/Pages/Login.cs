using Conduit.Domain;
using Radix;
using Radix.Data;
using static Radix.Control.Result.Extensions;

namespace Conduit.Components;

[Route("/Login")]
[InteractiveServerRenderMode(Prerender = true)]
public class Login : Component<LoginModel, LoginCommand>

{
    [Inject]
    public Domain.Login PerformLogin { get; set; } = (_, __) => Task.FromResult(Error<User, string>("Login function has not been added to dependency container"));

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Inject]
    public MessageBus MessageBus { get; set; } = null!;

    public override async ValueTask<LoginModel> Update(LoginModel model, LoginCommand command)
    {
        switch (command)
        {
            case SetLoginEmail(var email):
                model = model with { Email = email };
                break;
            case SetPasswordForLogin(var password):
                model = model with { Password = password };
                break;
            case LoginUser(var validatedCredentials):
                switch (validatedCredentials)
                {                
                    case Valid<Credentials>(var credentials):
                        var loginResponse = await PerformLogin(credentials.Email, credentials.Password);
                                
                        switch (loginResponse)
                        {
                            case Ok<User, string>(var user) when user != null:
                                MessageBus.Publish(new UserLoggedIn(user));
                                // Redirect to the home page
                                Navigation!.NavigateTo("/");
                                break;
                            case Error<Domain.User, string>(var error):
                                model = model with { Errors = [error] };
                                break;
                        }
                        break;
                    case Invalid<Credentials>(var errors):
                        model = model with { Errors = errors.Select(error => $"{error.Title}: {error.Descriptions.Aggregate((s, s1) => s + ", and " + s1)}").ToArray() };
                        break;
                }
                break;
        }
        return model;
    }

    public override Node[] View(LoginModel model, Func<LoginCommand, Task> dispatch)
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
                        ul([@class(["error-messages"])], 
                            model.Errors.Select(error => li([], [text(error)])).ToArray()
                        ),
                        form([], [
                            fieldset([@class(["form-group"])], [
                                input([@class(["form-control", "form-control-lg"]), type(["text"]), placeholder(["Email"]), on.change(a => dispatch(new SetLoginEmail((string?)a.Value)))], [])
                            ]),
                            fieldset([@class(["form-group"])], [
                                input([@class(["form-control", "form-control-lg"]), type(["password"]), placeholder(["Password"]), on.change(a => dispatch(new SetPasswordForLogin((string?)a.Value)))], [])
                            ]),
                            button([@class(["btn", "btn-lg", "btn-primary", "pull-xs-right"]), type(["button"]), 
                            on.click(args => {
                                model = model with { Errors = []};
                                dispatch(new LoginUser(Credentials.Create(model.Email, model.Password)));
                            })], [text("Sign in")])
                        ])
                    ])
                ])
            ])
        ])
    ];
}

[ValidatedMember<string, Radix.Data.String.Validity.IsNotNullEmptyOrWhiteSpace>("Email")]
[ValidatedMember<string, Radix.Data.String.Validity.IsNotNullEmptyOrWhiteSpace>("Password")]
public partial record Credentials;

public record LoginModel
{
    public string? Email { get; init; }
    public string? Password { get; init; }
    public string[] Errors { get; init; } = [];
}

public interface LoginCommand;
public record SetLoginEmail(string? Email) : LoginCommand;
public record SetPasswordForLogin(string? Password) : LoginCommand;
public record LoginUser(Validated<Credentials> validatedCredentials) : LoginCommand;
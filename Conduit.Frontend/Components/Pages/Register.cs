
using Microsoft.AspNetCore.Components.Web;
using Radix;
using Radix.Data;

using Conduit.Domain;
using static Radix.Control.Result.Extensions;


namespace Conduit.Components;

[Route("/Register")]
[InteractiveServerRenderMode(Prerender = true)]
public class Register : Component<RegisterModel, RegisterCommand>
{

    [Inject]
    public CreateUser CreateUser { get; set; } = (_, __, ___) => Task.FromResult(Error<Domain.User, string[]>(["CreateUser function has not been added to dependency container"]));
    
    [Inject]
    public NavigationManager? Navigation { get; set; }

    
    [Inject]
    public MessageBus MessageBus { get; set; } = null!;

    public override async ValueTask<RegisterModel> Update(RegisterModel model, RegisterCommand command)
    {
        switch (command)
        {
            case RegisterUser(var validatedRegistration):
                {
                    switch(validatedRegistration)
                    {
                        case Valid<Registration>(var registration):
                            {
                                try
                                {
                                    var createUserResponse = await CreateUser(model.UserName, model.Email, model.Password);
                                    switch(createUserResponse){
                                        case Ok<User, string[]> _:
                                            //var loginResponse = await Login(model.Email, model.Password);
                                            //switch(loginResponse){
                                            //    case Ok<Domain.User, string>(var user) when user != null:
                                            //        MessageBus.Publish(new UserLoggedIn(user));
                                            //        // Redirect to the home page
                                            //        Navigation!.NavigateTo("/");
                                            //        break;
                                            //    case Error<Domain.User, string>(var error):
                                            //        model = model with { Errors = [error] };
                                            //        break;
                                            //}     
                                            break;
                                        case Error<Domain.User, string[]>(var errors):
                                            model = model with { Errors = errors };
                                            break;
                                    }                           
                                }
                                catch (Exception e)
                                {
                                    model = model with { Errors = [e.Message] };
                                }
                            }
                            break;
                        case Invalid<Registration>(var invalidRegistration):
                            {
                                model = model with { Errors = invalidRegistration.Select(x => x.ToString()).ToArray()};
                            }
                            break;
                    }
                    
                }
                break;
            case SetUserNameForRegistration(var userName):
                model = model with { UserName = userName };
                break;
            case SetEmailForRegistration(var email):
                model = model with { Email = email };
                break;
            case SetPasswordForRegistration(var password):
                model = model with { Password = password };
                break;
        }
        
        return model;
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
                        ul([@class(["error-messages"])], 
                            model.Errors.Select(error => li([], [text(error)])).ToArray()
                        ),
                        form([], [
                            fieldset([@class(["form-group"])], [
                                input([@class(["form-control", "form-control-lg"]), type(["text"]), placeholder(["Username"]), value([model.UserName ?? ""]), on.change(a => dispatch(new SetUserNameForRegistration((string?)a.Value)))], [])
                            ]),
                            fieldset([@class(["form-group"])], [
                                input([@class(["form-control", "form-control-lg"]), type(["email"]), placeholder(["Email"]), value([model.Email ?? ""]), on.change(a => dispatch(new SetEmailForRegistration((string?)a.Value)))], [])
                            ]),
                            fieldset([@class(["form-group"])], [
                                input([@class(["form-control", "form-control-lg"]), type(["password"]), placeholder(["Password"]), value([model.Password ?? ""]), on.change(a => dispatch(new SetPasswordForRegistration((string?)a.Value)))], [])
                            ]),
                            button([@class(["btn", "btn-lg", "btn-primary", "pull-xs-right"]), type(["button"]), 
                            on.click(args => 
                            {
                                model = model with { Errors = []};
                                dispatch(new RegisterUser(Registration.Create(model.UserName, model.Email, model.Password)));
                            }
                            )], [text("Sign up")])
                        ])
                    ])
                ])
            ])
        ])
    ];
}


[ValidatedMember<string, Radix.Data.String.Validity.IsNotNullEmptyOrWhiteSpace>("UserName")]
[ValidatedMember<string, Radix.Data.String.Validity.IsNotNullEmptyOrWhiteSpace>("Email")]
[ValidatedMember<string, Radix.Data.String.Validity.IsNotNullEmptyOrWhiteSpace>("Password")]
public partial record Registration;

public interface RegisterCommand;
    public record SetUserNameForRegistration(string? UserName) : RegisterCommand;
    public record SetEmailForRegistration(string? Email) : RegisterCommand;
    public record SetPasswordForRegistration(string? Password) : RegisterCommand;
    public record RegisterUser(Validated<Registration> validatedRegistration) : RegisterCommand;

public record RegisterModel
{
    public string? UserName { get; init; } = "";
    public string? Email { get; init; } = "";
    public string? Password { get; init; } = "";
    public string[] Errors { get; init; } = [];
}

using Conduit.ApiClient;
using Microsoft.AspNetCore.Components.Web;
using Radix;
using Radix.Data;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Conduit.Components;



[Route("/Register")]
[InteractiveServerRenderMode]
public class Register : Component<RegisterModel, RegisterCommand>
{
    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Inject]
    public RealWorldClient? ApiClient { get; set; }

    [Inject]
    public ProtectedLocalStorage? LocalStorage { get; set; }

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
                                    var response = await ApiClient!.CreateUserAsync(new NewUserRequest
                                    {
                                        User = new NewUser
                                        {
                                            Username = model.UserName,
                                            Email = model.Email,
                                            Password = model.Password
                                        }
                                    });

                                    var loginResponse = await ApiClient.LoginAsync(new LoginUserRequest
                                    {
                                        User = new LoginUser
                                        {
                                            Email = model.Email,
                                            Password = model.Password
                                        }
                                    });

                                    // Store the user in protected local storage
                                    await LocalStorage!.SetAsync(LocalStorageKey.User, loginResponse.User);

                                    // Redirect to the home page
                                    Navigation!.NavigateTo("/");
                                }
                                catch (ApiException<GenericErrorModel> e)
                                {
                                    model = model with { Errors = e.Result.Errors.Select(error => $"{error.Key}: {error.Value.Aggregate((s, s1) => s + ", and " + s1)}").ToArray() };
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
            case SetUserName(var userName):
                model = model with { UserName = userName };
                break;
            case SetEmail(var email):
                model = model with { Email = email };
                break;
            case SetPassword(var password):
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
                                input([@class(["form-control", "form-control-lg"]), type(["text"]), placeholder(["Username"]), value([model.UserName ?? ""]), on.change(a => dispatch(new SetUserName((string?)a.Value)))], [])
                            ]),
                            fieldset([@class(["form-group"])], [
                                input([@class(["form-control", "form-control-lg"]), type(["email"]), placeholder(["Email"]), value([model.Email ?? ""]), on.change(a => dispatch(new SetEmail((string?)a.Value)))], [])
                            ]),
                            fieldset([@class(["form-group"])], [
                                input([@class(["form-control", "form-control-lg"]), type(["password"]), placeholder(["Password"]), value([model.Password ?? ""]), on.change(a => dispatch(new SetPassword((string?)a.Value)))], [])
                            ]),
                            button([@class(["btn", "btn-lg", "btn-primary", "pull-xs-right"]), type(["button"]), 
                            on.click(args => {
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

public record SetUserName(string? UserName) : RegisterCommand;
public record SetEmail(string? Email) : RegisterCommand;
public record SetPassword(string? Password) : RegisterCommand;
public record RegisterUser(Validated<Registration> Command) : RegisterCommand;


public record RegisterModel
{
    public string? UserName { get; init; } = "";
    public string? Email { get; init; } = "";
    public string? Password { get; init; } = "";
    public string[] Errors { get; init; } = [];
}

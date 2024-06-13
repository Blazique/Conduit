using Microsoft.AspNetCore.Authorization;

namespace Conduit.Components;

[Route("/Settings")]
[Authorize]
public class Settings : Blazique.Web.Component
{
    public override Node[] Render()
    =>
        [
            div([@class(["settings-page"])], [
                div([@class(["container", "page"])],[
                    div([@class(["row"])], [
                        div([@class(["col-md-6", "offset-md-3", "col-xs-12"])], [
                            h1([@class(["text-xs-center"])], [text("Your Settings")]),
                            ul([@class(["error-messages"])], [li([], [text("That name is required")])]),
                            form([], [
                                fieldset([], [
                                    fieldset([@class(["form-group"])], [
                                        input([@class(["form-control", "form-control-lg"]), type(["text"]), placeholder(["URL of profile picture"])], [])
                                    ]),
                                    fieldset([@class(["form-group"])], [
                                        input([@class(["form-control", "form-control-lg"]), type(["text"]), placeholder(["Your Name"])], [])
                                    ]),
                                    fieldset([@class(["form-group"])], [
                                        textarea([@class(["form-control", "form-control-lg"]), rows(["8"]), placeholder(["Short bio about you"])], [])
                                    ]),
                                    fieldset([@class(["form-group"])], [
                                        input([@class(["form-control", "form-control-lg"]), type(["text"]), placeholder(["Email"])], [])
                                    ]),
                                    fieldset([@class(["form-group"])], [
                                        input([@class(["form-control", "form-control-lg"]), type(["password"]), placeholder(["New Password"])], [])
                                    ]),
                                    button([@class(["btn", "btn-lg", "btn-primary", "pull-xs-right"]), type(["submit"])], [text("Update Settings")])
                                ])
                            ]),
                            button([@class(["btn", "btn-outline-danger"])], [text("Or click here to logout.")])
                        ])
                    ])
                ])
            ])
        ];
         
}


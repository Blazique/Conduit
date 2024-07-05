using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Conduit.Domain;
using static Radix.Control.Option.Extensions;
using static Radix.Control.Result.Extensions;
using static Radix.Control.Validated.Extensions;
using Radix;
using Radix.Data;

namespace Conduit.Components;

[Route("/Editor")]
[Authorize]
[InteractiveServerRenderMode(Prerender = true)]
public class Editor : Blazique.Web.Component<EditorPageModel, EditorPageCommand>
{
    [CascadingParameter]
    private Task<AuthenticationState>? AuthState { get; set; }

    private ClaimsPrincipal? _user;

    [Inject]
    public Domain.CreateArticle CreateArticle { get; set; } = (_, _, _, _) => Task.FromResult(Error<Domain.Article, string[]>([""]));

    public override Node[] View(EditorPageModel model, Func<EditorPageCommand, Task> dispatch)
    =>
    [
        div([@class(["editor-page"])], [
            div([@class(["container", "page"])], [
                div([@class(["row"])], [
                    div([@class(["col-md-10", "offset-md-1", "col-xs-12"])], [
                        ul([@class(["error-messages"])],
                            model.Errors.Select(error => li([], [text(error)])).ToArray()
                        ),
                        form([], [
                            fieldset([], [
                                fieldset([@class(["form-group"])], [
                                    input([@class(["form-control", "form-control-lg"]), type(["text"]), placeholder(["Article Title"]), on.change(a => dispatch(new SetTitleForArticle((string?)a.Value)))], [])
                                ]),
                                fieldset([@class(["form-group"])], [
                                    input([@class(["form-control", "form-control-lg"]), type(["text"]), placeholder(["What's this article about?"]), on.change(a => dispatch(new SetDescriptionForArticle((string?)a.Value)))], [])
                                ]),
                                fieldset([@class(["form-group"])], [
                                    textarea([@class(["form-control", "form-control-lg"]), rows(["8"]), placeholder(["Write your article (in markdown)"]), on.change(a => dispatch(new SetBodyForArticle((string?)a.Value)))], [])
                                ]),
                                fieldset([@class(["form-group"])], [
                                    input([@class(["form-control", "form-control-lg"]), type(["text"]), placeholder(["Enter tags"]), on.change(a => dispatch(new SetTagsForArticle((string?)a.Value)))], []),
                                    div([@class(["tag-list"])],
                                        !string.IsNullOrEmpty(model.Tags)
                                        ? FormatTags(model.Tags)
                                        : [empty()]
                                        )

                                ]),
                                button([@class(["btn", "btn-lg", "pull-xs-right", "btn-primary"]), type(["button"]),
                                on.click(args =>
                                {
                                    model = model with { Errors = []};
                                    dispatch(new CreateArticle(NewArticle.Create(model.Title, model.Description, model.Body, model.Tags)));
                                })], [text("Publish Article")])
                            ])
                        ])
                    ])
                ])
            ])
        ])
    ];

    private static Node[] FormatTags(string tags)
     => tags
        .Split(' ')
        .Select(tag =>
                span([@class(["tag-default", "tag-pill"])], [
                    i([@class(["ion-close-round"])], []),
                    text(tag)])).ToArray();

    public override async ValueTask<EditorPageModel> Update(EditorPageModel model, EditorPageCommand command)
    {
        switch (command)
        {
            case SetTitleForArticle(var title):
                {
                    model = model with { Title = title };
                    break;
                }
            case SetDescriptionForArticle(var description):
                {
                    model = model with { Description = description };
                    break;
                }
            case SetBodyForArticle(var body):
                {
                    model = model with { Body = body };
                    break;
                }
            case SetTagsForArticle(var tags):
                {
                    model = model with { Tags = tags };
                    break;
                }
            case CreateArticle(var validated):
                {
                    switch (validated)
                    {
                        case Invalid<NewArticle>(var errors):
                            {
                                model = model with { Errors = errors.Select(x => x.ToString()).ToArray() };
                                break;
                            }
                        case Valid<NewArticle>(var newArticle):
                            {
                                var articleResponse = await CreateArticle(newArticle.Title, newArticle.Description, newArticle.Body, newArticle.Tags.Split(' ').Select(s => (Tag)s).ToArray());
                                model.Errors = [];
                                break;
                            }
                    }
                    break;
                }
            default:
                {
                    return model;
                }
        }
        return model;
    }
}

internal record CreateArticle(Validated<NewArticle> Validated) : EditorPageCommand;

public record PassTroughValidity : Radix.Data.Validity<string>
{
    public static Func<string, Func<string, Validated<string>>> Validate => _ => static o => Valid(o);
}

[ValidatedMember<string, Radix.Data.String.Validity.IsNotNullEmptyOrWhiteSpace>("Title")]
[ValidatedMember<string, Radix.Data.String.Validity.IsNotNullEmptyOrWhiteSpace>("Description")]
[ValidatedMember<string, Radix.Data.String.Validity.IsNotNullEmptyOrWhiteSpace>("Body")]
[ValidatedMember<string, PassTroughValidity>("Tags")]
public partial record NewArticle;

internal record SetDescriptionForArticle(string? Value) : EditorPageCommand;

internal record SetTitleForArticle(string? Value) : EditorPageCommand;
internal record SetBodyForArticle(string? Value) : EditorPageCommand;

internal record SetTagsForArticle(string? Value) : EditorPageCommand;


public interface EditorPageCommand;

public record EditorPageModel
{
    public string? Title { get; internal set; }
    public string? Description { get; internal set; }
    public string? Body { get; internal set; }
    public string? Tags { get; internal set; }
    public string[] Errors { get; internal set; } = [];
}
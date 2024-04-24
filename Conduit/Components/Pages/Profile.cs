
using Conduit.ApiClient;
using Conduit.Domain;
using Radix.Data;
using static Radix.Control.Option.Extensions;

namespace Conduit.Components;

[Route("/Profile/{username}")]
[InteractiveServerRenderMode(Prerender = false)]
public class Profile : Component<ProfilePageModel, ProfilePageCommand>
{
    [Inject]
    public GetProfile GetProfile { get; set; } = (_) => Task.FromResult(None<Domain.Profile>());

    [Inject]
    public GetUser GetUser { get; set; } = () => Task.FromResult(None<Domain.User>());

    [Inject]
    public GetArticlesFeed GetArticlesFeed { get; set; } = (_, _, _) => Task.FromResult(new ArticleFeed(0,[]));

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Parameter]
    public string Username { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        Model.PageSize = 10;
        Model.Page = 1;
        switch (await GetProfile(Username))
        {
            case Some<Domain.Profile>(var profile):
                Model.Profile = profile;
                break;
            case None<Domain.Profile>:
                break;
        }
        switch (await GetUser())
        {
            case Some<Domain.User>(var user):
                Model.User = user;
                Model.Feed = await GetArticlesFeed(user.Token, Model.PageSize, 0);
                Model.TotalPages = (Model.Feed.ArticlesCount +  Model.PageSize - 1) /  Model.PageSize;
                break;
            case None<Domain.User>:
                break;
        }
    }

    public override Node[] View(ProfilePageModel model, Func<ProfilePageCommand, Task> dispatch)
    => 
        model.Profile is not null ? 
            [        
                div([@class(["profile-page"])], [
                    div([@class(["user-info"])], [
                        div([@class(["container"])], [
                            div([@class(["row"])], [
                                div([@class(["col-xs-12", "col-md-10", "offset-md-1"])], [
                                    img([@class(["user-img"]), src([model.Profile.Image])], []),
                                    h4([], [text(model.Profile.Username)]),
                                    p([], [text(model.Profile.Bio ?? "")]),
                                    button([@class(["btn", "btn-sm", "btn-outline-secondary", "action-btn"])], 
                                        [i([@class(["ion-plus-round"])], [text($" Follow {model.Profile.Username}")])]),
                                    button([@class(["btn", "btn-sm", "btn-outline-secondary", "action-btn"])], 
                                        [i([@class(["ion-gear-a"]), on.click(_ => Navigation?.NavigateTo("/settings"))], [text(" Edit Profile Settings")]),

                                ])])
                            ])])])]),
                div([@class(["container"])], [
                    div([@class(["row"])], [
                        div([@class(["col-xs-12", "col-md-10", "offset-md-1"])], [
                            div([@class(["articles-toggle"])], [
                                ul([@class(["nav", "nav-pills", "outline-active"])], [
                                    li([@class(["nav-item"])], [
                                        a([@class(["nav-link", "active"]), href([""])], [text("My Articles")])
                                    ]),
                                    li([@class(["nav-item"])], [
                                        a([@class(["nav-link"]), href([""])], [text("Favorited Articles")])
                                    ])
                                ])
                            ]),
                            .. model.Feed is not null 
                                ? model.Feed.Articles.Select(article =>
                                    div([@class(["article-preview"])], [
                                        div([@class(["article-meta"])], [
                                            a([href([$"/profile/{article.Author.Username}"])], [
                                                img([src([article.Author.Image])], [])]),
                                            div([@class(["info"])], [
                                                a([@class(["author"]), href([$"/profile/{article.Author.Username}"])], [
                                                    text(article.Author.Username)]),],
                                                span([@class(["date"])], [text("January 20th")])),
                                            button([@class(["btn", "btn-outline-primary", "btn-sm", "pull-xs-right"])], [
                                                i([@class(["ion-heart"])], []),
                                        ]),
                                        a([href(["/article/how-to-buil-webapps-that-scale"]), @class(["preview-link"])], [
                                            h1([], [text(article.Title)]),
                                            p([], [text(article.Description)]),
                                            span([], [text("Read more...")]),
                                            ul([@class(["tag-list"])], article.TagList.Select(tag =>
                                                li([@class(["tag-pill", "tag-default"])], [text(tag)])).ToArray()),
                                                
                                        ])
                                    ])])).ToArray()
                                : [],
                            ul([@class(["pagination"])], Enumerable.Range(1, model.TotalPages).Select(page =>
                                li([@class(["page-item", page == model.Page ? "active" : ""])], [
                                    button([@class(["page-link"]), on.click((_) => dispatch(new ChangeProfileFeedPage(page)))], [text(page.ToString())])
                                ])
                            ).ToArray())
                        ])
                    ])
                ])
            ]
        :
            [
                div([@class(["alert", "alert-warning"])], [text("Profile not found")])
            ];

        public override async ValueTask<ProfilePageModel> Update(ProfilePageModel model, ProfilePageCommand command)
        {
            switch (command)
            {
                case ChangeProfileFeedPage(var page):
                    model.Page = page;
                    model.Feed = Model.User is not null ? await GetArticlesFeed(Model.User.Token, Model.PageSize, (Model.Page - 1) * Model.PageSize) : new ArticleFeed(0, []);
                    break;
            }
            return model;
        }
    }

public interface ProfilePageCommand;

internal record ChangeProfileFeedPage(int Page) : ProfilePageCommand;

public record ProfilePageModel
{
    public Domain.Profile? Profile { get; internal set; } 
    public Domain.User? User { get; internal set; }
    public int PageSize { get; internal set; }
    public int Page { get; internal set; }
    public ArticleFeed? Feed { get; internal set; }
    public int TotalPages { get; internal set; }
}
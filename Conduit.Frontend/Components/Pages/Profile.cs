

using Conduit.Domain;
using k8s.KubeConfigModels;
using Microsoft.AspNetCore.Components.Authorization;
using Radix.Data;
using System.Security.Claims;
using static Radix.Control.Option.Extensions;

namespace Conduit.Components;

[Route("/Profile/{username}")]
[InteractiveServerRenderMode(Prerender = false)]
public class Profile : Component<ProfilePageModel, ProfilePageCommand>
{
    private ClaimsPrincipal? _user;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthState { get; set; }

    [Inject]
    public GetProfile GetProfile { get; set; } = (_) => Task.FromResult(None<Domain.Profile>());

    [Inject]
    public ListArticles ListArticles { get; set; } = (_, _, _, _, _) => Task.FromResult(new ArticleFeed(0,[]));

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Inject]
    public MarkArticleAsFavorite MarkArticleAsFavorite { get; set; } = (_) => Task.CompletedTask;

    [Inject]
    public UnmarkArticleAsFavorite UnmarkArticleAsFavorite { get; set; } = (_) => Task.CompletedTask;

    [Parameter]
    public string Username { get; set; } = "";

    protected override async Task<ProfilePageModel> Initialize(ProfilePageModel model)
    {
        if (AuthState == null)
        {
            return model;
        }

        AuthenticationState authState = await AuthState;
        _user = authState.User;

        model.PageSize = 10;
        model.Page = 1;
        switch (await GetProfile(Username))
        {
            case Some<Domain.Profile>(var profile):
                model.Profile = profile;
                break;
            case None<Domain.Profile>:
                break;
        }
        model.Feed = await ListArticles(Model.PageSize, (Model.Page - 1) * Model.PageSize, "", Model.Profile.Username, "");
        return model;
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
                                        a([@class(["nav-link", model.SelectedFeed == SelectedProfileFeed.MyFeed ? "active" : ""]), on.click(_ => dispatch(new SetProfileFeed(SelectedProfileFeed.MyFeed)))], [text("My Articles")])
                                    ]),
                                    li([@class(["nav-item"])], [
                                        a([@class(["nav-link", model.SelectedFeed == SelectedProfileFeed.FavoritedFeed ? "active" : ""]), on.click(_ => dispatch(new SetProfileFeed(SelectedProfileFeed.FavoritedFeed)))], [text("Favorited Articles")])
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
                                                    text(article.Author.Username)]),
                                                span([@class(["date"])], [text(article.CreatedAt.FormatWithOrdinal())])]),
                                            button([@class(["btn", "btn-outline-primary", "btn-sm", "pull-xs-right"]), on.click(_ => dispatch(new InvertMarkArticleAsFavorite(article)))], [
                                                i([@class(["ion-heart"])], []),
                                                text($" {article.FavoritesCount}")])
                                        ]),
                                        a([href([$"/article/{article.Slug}"]), @class(["preview-link"])], [
                                            h1([], [text(article.Title)]),
                                            p([], [text(article.Description)]),
                                            span([], [text("Read more...")]),
                                            ul([@class(["tag-list"])], article.TagList.Select(tag =>
                                                li([@class(["tag-pill", "tag-default", "tag-outline"])], [text(tag)])).ToArray()),
                                                
                                        ])
                                    ])).ToArray()
                                : [div([@class(["spinner-border"]), role(["status"])], [
                                        span([@class(["sr-only"])], [text("Loading...")])
                                    ])],
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
                    await RefreshFeed(model);
                break;
                case SetProfileFeed setFeed:
                    model.SelectedFeed = setFeed.SelectedFeed;
                    model.Page = 1;
                    await RefreshFeed(model);
                    model.TotalPages = model.Feed is not null ? (model.Feed.ArticlesCount + model.PageSize - 1) / model.PageSize : 0;
                    break;
                case InvertMarkArticleAsFavorite invertMarkArticleAsFavorite:
                    if (_user.Identity.IsAuthenticated)
                    {
                        string userIdentityValue = _user.Claims.FirstOrDefault(claim => claim.Type == "sub").Value;
                        if (invertMarkArticleAsFavorite.Article.FavoritedBy.Contains(userIdentityValue))
                        {
                            await UnmarkArticleAsFavorite(invertMarkArticleAsFavorite.Article.Slug);
                        }
                        else
                        {
                            await MarkArticleAsFavorite(invertMarkArticleAsFavorite.Article.Slug);
                        }

                        await RefreshFeed(model);
                    }
                    break;
        }
            async Task RefreshFeed(ProfilePageModel model)
            {
                switch (model.SelectedFeed)
                {
                    case SelectedProfileFeed.MyFeed:
                        model.Feed = await ListArticles(Model.PageSize, (Model.Page - 1) * Model.PageSize, null, Model.Profile.Username, null);
                        break;
                    case SelectedProfileFeed.FavoritedFeed:
                        model.Feed = await ListArticles(model.PageSize, (model.Page - 1) * model.PageSize, null, null, Model.Profile.Username);
                        break;
                }
            }
            return model;
        }
    }

public interface ProfilePageCommand;

internal record SetProfileFeed : ProfilePageCommand
{
    public SetProfileFeed(SelectedProfileFeed selectedFeed)
    {
        SelectedFeed = selectedFeed;
    }

    public SelectedProfileFeed SelectedFeed { get; }
}

internal record ChangeProfileFeedPage(int Page) : ProfilePageCommand;

public record ProfilePageModel
{
    public Domain.Profile? Profile { get; internal set; } 
    public int PageSize { get; internal set; }
    public int Page { get; internal set; }
    public ArticleFeed? Feed { get; internal set; }
    public int TotalPages { get; internal set; }
    public SelectedProfileFeed SelectedFeed { get; internal set; }
}

public enum SelectedProfileFeed
{
    MyFeed,
    FavoritedFeed
}
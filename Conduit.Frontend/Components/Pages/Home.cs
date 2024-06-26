
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Blazique.Web.Html.Names.Elements;
using Conduit.Domain;
using k8s.KubeConfigModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Radix.Data;
using static Radix.Control.Option.Extensions;

namespace Conduit.Components;

[Route("/")]
[InteractiveServerRenderMode(Prerender = false)]
public class Home : Component<HomePageModel, HomePageCommand>
{
    private ClaimsPrincipal? _user;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthState { get; set; }

    [Inject]
    public ListArticles ListArticles { get; set; } = (_, _, _, _, _) => Task.FromResult(new ArticleFeed(0, []));

    [Inject]
    public GetArticlesFeed GetArticlesFeed { get; set; } = (_, _) => Task.FromResult(new ArticleFeed(0, []));

    [Inject]
    public MarkArticleAsFavorite MarkArticleAsFavorite { get; set; } = (_) => Task.CompletedTask;

    [Inject]
    public UnmarkArticleAsFavorite UnmarkArticleAsFavorite { get; set; } = (_) => Task.CompletedTask;

    [Inject]
    public GetTags GetTags { get; set; } = () => Task.FromResult(Array.Empty<string>());


    protected override async Task<HomePageModel> Initialize(HomePageModel model) 
    {
        if (AuthState == null)
        {
            return model;
        }

        AuthenticationState authState = await AuthState;
        _user = authState.User;

        model.SelectedFeed = SelectedHomeFeed.GlobalFeed;
        model.PageSize = 10;
        model.Page = 1;
        model.Tags = await GetTags();

        if(_user is not null && _user.Identity.IsAuthenticated)
        {
            model.SelectedFeed = SelectedHomeFeed.YourFeed;
            model.Feed = await GetArticlesFeed(model.PageSize, (model.Page - 1) * model.PageSize);
        }
        else
        {
            model.Feed = await ListArticles(model.PageSize, 0);
            
        }
        model.TotalPages = (model.Feed.ArticlesCount + model.PageSize - 1) / model.PageSize;
        return model;
    }

    public override async ValueTask<HomePageModel> Update(HomePageModel model, HomePageCommand command)
    {
        switch (command)
        {
            case ChangeHomeFeedPage changeHomeFeedPage:
                model.Page = changeHomeFeedPage.Page;
                await RefreshFeed(model);
                break;
            case SetHomeFeed setFeed:
                model.SelectedFeed = setFeed.SelectedFeed;
                model.Page = 1;
                await RefreshFeed(model);
                model.TotalPages = model.Feed is not null ? (model.Feed.ArticlesCount + model.PageSize - 1) / model.PageSize : 0;
                break;
            case SelectPopularTag selectPopularTag:
                model.SelectedPopularTag = selectPopularTag.Tag;
                model.SelectedFeed = SelectedHomeFeed.SelectedPopularTag;
                model.Page = 1;
                model.Feed = await ListArticles(model.PageSize, (model.Page - 1) * model.PageSize, model.SelectedPopularTag);
                model.TotalPages = model.Feed is not null ? (model.Feed.ArticlesCount + model.PageSize - 1) / model.PageSize : 0;
                break;
            case InvertMarkArticleAsFavorite invertMarkArticleAsFavorite:
                if(_user.Identity.IsAuthenticated)
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
        return model;

        async Task RefreshFeed(HomePageModel model)
        {
            switch (model.SelectedFeed)
            {
                case SelectedHomeFeed.YourFeed:
                    model.Feed = await GetArticlesFeed(model.PageSize, (model.Page - 1) * model.PageSize);
                    break;
                case SelectedHomeFeed.GlobalFeed:
                    model.Feed = await ListArticles(model.PageSize, (model.Page - 1) * model.PageSize);
                    break;
                case SelectedHomeFeed.SelectedPopularTag:
                    model.Feed = await ListArticles(model.PageSize, (model.Page - 1) * model.PageSize, model.SelectedPopularTag);
                    break;
            }
        }
    }

    public override Node[] View(HomePageModel model, Func<HomePageCommand, Task> dispatch)
    =>
    [
        div([@class(["home-page"])], [
            div([@class(["banner"])], [
                div([@class(["container"])], [
                    h1([], [text("conduit")]),
                    p([], [text("A place to share your knowledge.")])
                ])])]),
        div([@class(["container page"])], [
            div([@class(["row"])], [
                div([@class(["col-md-9"])], [
                    div([@class(["feed-toggle"])], [
                        ul([@class(["nav", "nav-pills", "outline-active"])], [
                            _user is not null && _user.Identity.IsAuthenticated
                            ? li([@class(["nav-item"])], [
                                a([@class(["nav-link", model.SelectedFeed == SelectedHomeFeed.YourFeed ? "active" : ""]), href([""]), on.click(_ => dispatch(new SetHomeFeed(SelectedHomeFeed.YourFeed)))], [text("Your Feed")])
                                ])
                            : empty(),
                            li([@class(["nav-item"])], [
                                a([@class(["nav-link", model.SelectedFeed == SelectedHomeFeed.GlobalFeed ? "active" : ""]), href([""]), on.click(_ => dispatch(new SetHomeFeed(SelectedHomeFeed.GlobalFeed)))], [text("Global Feed")])
                            ]),
                            !string.IsNullOrEmpty(model.SelectedPopularTag) ? li([@class(["nav-item"])], [
                                a([@class(["nav-link", model.SelectedFeed == SelectedHomeFeed.SelectedPopularTag ? "active" : ""]), href([""]), on.click(_ => dispatch(new SetHomeFeed(SelectedHomeFeed.SelectedPopularTag)))], [text($"#{model.SelectedPopularTag}")])
                            ]) : empty()
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
                            button([@class(["page-link"]), on.click((_) => dispatch(new ChangeHomeFeedPage(page)))], [text(page.ToString())])
                        ])
                    ).ToArray()),
                    
                ]),
                div([@class(["col-md-3"])], [
                    div([@class(["sidebar"])], [
                        p([], [text("Popular Tags")]),
                        div([@class(["tag-list"])], [
                            .. model.Tags is not null
                            ? model.Tags.Select(tag => a([@class(["tag-pill", "tag-default"]), attribute("style", ["cursor:hand"]), on.click(_ => dispatch(new SelectPopularTag(tag)))], [text(tag)])).ToArray()
                            : []
                        ])
                    ])
                ])
            ])
        ])
    ];
}

internal record InvertMarkArticleAsFavorite(Domain.Article Article) : HomePageCommand, ProfilePageCommand;

internal record SelectPopularTag(string Tag) : HomePageCommand;

internal record SetHomeFeed : HomePageCommand
{
    public SetHomeFeed(SelectedHomeFeed selectedFeed)
    {
        SelectedFeed = selectedFeed;
    }

    public SelectedHomeFeed SelectedFeed { get; }
}

public interface HomePageCommand;

internal record ChangeHomeFeedPage(int Page) : HomePageCommand;

public record HomePageModel
{
    public SelectedHomeFeed SelectedFeed { get; internal set; }
    public int PageSize { get; internal set; }
    public int Page { get; internal set; }
    public ArticleFeed? Feed { get; internal set; }
    public int TotalPages { get; internal set; }
    public string? SelectedPopularTag { get; internal set; }
    public string[]? Tags { get; internal set; }
}

public enum SelectedHomeFeed
{
    YourFeed,
    GlobalFeed,
    SelectedPopularTag
}
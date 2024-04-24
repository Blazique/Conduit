
using Blazique.Web.Html.Names.Elements;
using Conduit.Domain;
using Radix.Data;
using static Radix.Control.Option.Extensions;

namespace Conduit.Components;

[Route("/")]
[InteractiveServerRenderMode(Prerender = false)]
public class Home : Component<HomePageModel, HomePageCommand>
{   

    [Inject]
    public GetUser GetUser { get; set; } = () => Task.FromResult(None<Domain.User>());

    [Inject]
    public GetAllRecentArticles GetAllRecentArticlesFeed { get; set; } = (_, _) => Task.FromResult(new ArticleFeed(0,[]));

    [Inject]
    public GetArticlesFeed GetArticlesFeed { get; set; } = (_, _, _) => Task.FromResult(new ArticleFeed(0,[]));

    protected override async Task OnInitializedAsync()
    {
        Model.SelectedFeed = SelectedFeed.GlobalFeed;
        Model.PageSize = 10;
        Model.Page = 1;
        Model.Feed = await GetAllRecentArticlesFeed(Model.PageSize, 0);
        Model.TotalPages = (Model.Feed.ArticlesCount +  Model.PageSize - 1) /  Model.PageSize;

        switch (await GetUser())
        {
            case Some<User>(User user):
                Model.User = user;
                Model.Feed = await GetAllRecentArticlesFeed(Model.PageSize, (Model.Page - 1) * Model.PageSize);
                Model.TotalPages = (Model.Feed.ArticlesCount +  Model.PageSize - 1) /  Model.PageSize;
                break;
            case None<User>:
                break;
        }
    }

    public override async ValueTask<HomePageModel> Update(HomePageModel model, HomePageCommand command)
    {
        switch(command)
        {
            case ChangeHomeFeedPage changeHomeFeedPage:
                model.Page = changeHomeFeedPage.Page;
                switch(Model.SelectedFeed)
                {
                    case SelectedFeed.YourFeed:
                        model.Feed = await GetArticlesFeed(Model.User.Token, Model.PageSize, (Model.Page - 1) * Model.PageSize);
                        break;
                    case SelectedFeed.GlobalFeed:
                        model.Feed = await GetAllRecentArticlesFeed(model.PageSize, (model.Page - 1) * model.PageSize);
                        break;
                }
                break;
            case SetFeed setFeed:
                model.SelectedFeed = setFeed.SelectedFeed;
                model.Page = 1;
                switch(model.SelectedFeed)
                {
                    case SelectedFeed.YourFeed:
                        model.Feed = await GetArticlesFeed(Model.User.Token, Model.PageSize, (Model.Page - 1) * Model.PageSize);
                        break;
                    case SelectedFeed.GlobalFeed:
                        model.Feed = await GetAllRecentArticlesFeed(model.PageSize, 0);
                        break;
                }
                model.TotalPages = model.Feed is not null ? (model.Feed.ArticlesCount +  model.PageSize - 1) /  Model.PageSize : 0;
                break;
        }
        return model;
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
                            li([@class(["nav-item"])], [
                                a([@class(["nav-link", model.SelectedFeed == SelectedFeed.YourFeed ? "active" : ""]), attribute("style", ["cursor:hand"]), on.click(_ => dispatch(new SetFeed(SelectedFeed.YourFeed)))], [text("Your Feed")])
                            ]),
                            li([@class(["nav-item"])], [
                                a([@class(["nav-link", model.SelectedFeed == SelectedFeed.GlobalFeed ? "active" : ""]), attribute("style", ["cursor:hand"]), on.click(_ => dispatch(new SetFeed(SelectedFeed.GlobalFeed)))], [text("Global Feed")])
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
                                        a([href([$"/article/{article.Slug}"]), @class(["preview-link"])], [
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
                                    button([@class(["page-link"]), on.click((_) => dispatch(new ChangeHomeFeedPage(page)))], [text(page.ToString())])
                                ])
                            ).ToArray())
                    ,
                    div([@class(["col-md-3"])], [
                        div([@class(["sidebar"])], [
                            p([], [text("Popular Tags")]),
                            div([@class(["tag-list"])], [
                                a([@class(["tag-pill", "tag-default"]), href([""])], [text("programming")]),
                                a([@class(["tag-pill", "tag-default"]), href([""])], [text("javascript")]),
                                a([@class(["tag-pill", "tag-default"]), href([""])], [text("emberjs")]),
                                a([@class(["tag-pill", "tag-default"]), href([""])], [text("angularjs")]),
                                a([@class(["tag-pill", "tag-default"]), href([""])], [text("react")]),
                                a([@class(["tag-pill", "tag-default"]), href([""])], [text("mean")]),
                                a([@class(["tag-pill", "tag-default"]), href([""])], [text("node")]),
                                a([@class(["tag-pill", "tag-default"]), href([""])], [text("rails")])
                            ])
                        ])
                    ])
                ])
            ])
        ])
    ];
}

internal record SetFeed : HomePageCommand
{
    public SetFeed(SelectedFeed selectedFeed)
    {
        SelectedFeed = selectedFeed;
    }

    public SelectedFeed SelectedFeed { get; }
}

public interface HomePageCommand;

internal record ChangeHomeFeedPage(int Page) : HomePageCommand;

public record HomePageModel
{
    public SelectedFeed SelectedFeed { get; internal set; }
    public int PageSize { get; internal set; }
    public int Page { get; internal set; }
    public ArticleFeed? Feed { get; internal set; }
    public int TotalPages { get; internal set; }
    public User User { get; internal set; }
}

public enum SelectedFeed
{
    YourFeed,
    GlobalFeed
}

using System.Diagnostics.Contracts;
using Conduit.Domain;
using Microsoft.AspNetCore.Components.Rendering;
using Radix.Data;
using static Radix.Control.Option.Extensions;
using static Radix.Control.Result.Extensions;

namespace Conduit.Components
{
    [Route("/article/{slug}")]
    [InteractiveServerRenderMode(Prerender = false)]
    public class Article : Component<ArticlePageModel, ArticlePageCommand>
    {
        private Timer? debounceTimer = new(_ => { }, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        [Parameter]
        public string Slug { get; set; } = "";

        [Inject]
        public GetArticle GetArticle { get; set; } = (_) => Task.FromResult(Error<Domain.Article, string[]>(["Article not found"]));

        [Inject]
        public GetUser GetUser { get; set; } = () => Task.FromResult(None<User>());

        [Inject]
        public GetComments GetComments { get; set; } = (_) => Task.FromResult(new List<Comment>());

        [Inject]
        public AddComment AddComment { get; set; } = (_, _, _) => Task.FromResult(Error<Comment, string[]>(["Comment not added"]));

        [Inject]
        public DeleteComment DeleteComment {get;set;} = (_, _, _) => Task.FromResult(Error<Unit, string[]>(["Comment not deleted"]));

        [Inject]
        public MarkArticleAsFavorite MarkArticleAsFavorite { get; set; } = (_, _) => Task.CompletedTask;

        [Inject]
        public UnmarkArticleAsFavorite UnmarkArticleAsFavorite { get; set; } = (_, _) => Task.CompletedTask;

        [Inject]
        public FollowUser FollowUser { get; set; } = (_, _) => Task.FromResult(Error<Unit, string[]>(["User not followed"]));

        [Inject]
        public UnfollowUser UnfollowUser { get; set; } = (_, _) => Task.FromResult(Error<Unit, string[]>(["User not unfollowed"]));

        [Inject]
        public NavigationManager? Navigation { get; set; }

        protected override async Task<ArticlePageModel> Initialize(ArticlePageModel model)
        {
            var userOption = await GetUser();
            User user = null!;
            switch (userOption)
            {
                case Some<User>(var usr):
                    user = usr;
                    break;
                case None<User>: break;
            }

            var article = await GetArticle((Slug)Slug);
            
            switch (article)
            {
                case Ok<Domain.Article, string[]>(var a):
                    var comments = await GetComments((Slug)a.Slug);
                    return model with { Article = a, User = user, Comments = comments};
                case Error<Domain.Article, string[]> (var errors):
                    return model with {Errors = errors};
            }
            
            return model;
        }

        public override async ValueTask<ArticlePageModel> Update(ArticlePageModel model, ArticlePageCommand command)
        {
            switch (command)
            {
                case EditArticleCommand _:
                    Navigation?.NavigateTo($"/editor/{model.Article.Slug}");
                    break;
                case DeleteArticleCommand _:
                    Navigation.NavigateTo($"/");
                    break;
                case InvertMarkArticleAsFavoriteCommand _:
                    if(model.Article.Favorited)
                    {
                        await UnmarkArticleAsFavorite(model.Article.Slug, model.User!.Token);
                    }
                    else
                    {
                        await MarkArticleAsFavorite(model.Article.Slug, model.User!.Token);
                    }
                    
                    return model with {Article = model.Article with {Favorited = !model.Article.Favorited, FavoritesCount = model.Article.FavoritesCount + (model.Article.Favorited ? -1 : 1)}};
                case FollowCommand _:
                    if(model.Article.Author.Following)
                    {
                        await UnfollowUser(model.Article.Author.Username, model.User!.Token);
                    }
                    else
                    {
                        await FollowUser(model.Article.Author.Username, model.User!.Token);
                    }
                    return model with {Article = model.Article with {Author = model.Article.Author with {Following = !model.Article.Author.Following}}};
                case PostCommentCommand (var comment):
                    var addCommentResult = await AddComment((Slug)model.Article.Slug, comment, model.User!.Token);
                    switch (addCommentResult)
                    {
                        case Ok<Comment, string[]> (var newComment):
                            return model with {Comments = [.. model.Comments, newComment]};
                        case Error<Comment, string[]> (var errors):
                            return model with {Errors = errors};
                    }
                    break;
                case SetNewComment(var value):
                    return model with {NewComment = value.ToString()};
                case DeleteCommentCommand(var id):
                    var deleteCommentResult = await DeleteComment((Slug)model.Article.Slug, id, model.User!.Token);
                    switch(deleteCommentResult)
                    {
                        case Ok<Unit, string[]> _:
                            return model with {Comments = model.Comments.Where(comment => comment.Id != id).ToList()};
                        case Error<Unit, string[]> (var errors):
                            return model with {Errors = errors};
                    }
                    break;
            }
            return model;
        }

        public override Node[] View(ArticlePageModel model, Func<ArticlePageCommand, Task> dispatch)
        {
            return model.Errors.Length != 0
                ? model.Errors.Select(error => div([@class(["alert", "alert-warning"])], [text(error)])).ToArray()
                : [
                    div([@class(["article-page"])], [
                div([@class(["banner"])], [
                    div([@class(["container"])], [
                        h1([], [text(model.Article.Title)]),
                        div([@class(["article-meta"])], ArticleMeta(model, dispatch))
                    ])
                ]),
                div([@class(["container", "page"])],[
                    div([@class(["row", "article-content"])], [
                        div([@class(["col-md-12"])],[
                            p([], [text(model.Article.Body)]),
                            ul([@class(["tag-list"])],
                                model.Article.TagList.Select(tag =>
                                    li([@class(["tag-default", "tag-pill", "tag-outline"])], [text(tag)])
                                ).ToArray()),
                        ])
                    ])
                ]),
                hr([], []),
                div([@class(["article-actions"])], [
                    div([@class(["article-meta"])], 
                        ArticleMeta(model, dispatch)),
                        model.User is not null
                        ?
                            div([@class(["row"])], [
                                div([@class(["col-xs-12", "col-md-8", "offset-md-2"])], [
                                    form([@class(["card", "comment-form"])], [
                                        div([@class(["card-block"])], [
                                                textarea([@class(["form-control"]), rows(["3"]), placeholder(["Write a comment..."]), on.input(async args => await dispatch(new SetNewComment(args.Value.ToString()))                                                  
                                                )], [])
                                            ]),
                                        div([@class(["card-footer"])], [
                                            img([src([model.User.Image]), @class(["comment-author-img"])], []),
                                            button([@class(["btn", "btn-sm", "btn-primary"]), type(["button"]), on.click(async _ => await dispatch(new PostCommentCommand(model.NewComment)))], [text("Post Comment")])
                                        ])
                                    ])
                                ])
                            ])
                        : empty(),
                        .. model.Comments.Select(comment => 
                            
                            div([@class(["card"])], [
                                div([@class(["card-block"])], [
                                    p([@class(["card-text"])], [text(comment.Body)]),
                                ]),
                                div([@class(["card-footer"])], [
                                    a([@class(["comment-author"]), href([$"/profile/{comment.Author.Username}"])], [
                                        img([src([comment.Author.Image]), @class(["comment-author-img"])], [])
                                    ]),
                                    text(" "),
                                    a([@class(["comment-author"]), href([$"/profile/{comment.Author.Username}"])], [text(comment.Author.Username)]),
                                    span([@class(["date-posted"])], [text(comment.CreatedAt.Year == DateTime.Now.Year ? FormatDateWithOrdinal(comment.CreatedAt) : FormatDateWithOrdinal(comment.CreatedAt) + " " + comment.CreatedAt.ToString("yyyy"))]),
                                    CurrentUserIsCommentAuthor(model, comment) 
                                    ?   span([@class(["mod-options"])], [
                                            button([@class(["ion-edit"])], []),
                                            button([@class(["ion-trash-a"]), on.click(_ => dispatch(new DeleteCommentCommand(comment.Id)))], [])
                                        ])
                                    : empty()
                                    
                                ])
                            ]) 
                        ).ToArray(),
                        
                    ])
                ])
            ];
        }
        
        public static string FormatDateWithOrdinal(DateTimeOffset date)
        {
            string suffix = (date.Day % 100) switch
            {
                11 or 12 or 13 => "th",
                _ => (date.Day % 10) switch
                {
                    1 => "st",
                    2 => "nd",
                    3 => "rd",
                    _ => "th",
                },
            };
            return string.Format("{0:MMM} {1}{2}", date, date.Day, suffix);
}

        private static Node[] ArticleMeta(ArticlePageModel model, Func<ArticlePageCommand, Task> dispatch)
        => [
                a([href([$"/profile/{model.Article.Author.Username}"])], [
                    img([src([model.Article.Author.Image])], [])
                ]),
                div([@class(["info"])], [
                    a([@class(["author"]), href([$"/profile/{model.Article.Author.Username}"])], [text(model.Article.Author.Username)]),
                    span([@class(["date"])], [text(model.Article.CreatedAt.ToString("MMMM d, yyyy"))])
                ]),
                .. CurrentUserIsArticleAuthor(model)
                ? ShowEditAndDeleteButtons(model, dispatch)
                : ShowFollowAndFavoriteButtons(model, dispatch)

            ];

        private static bool CurrentUserIsArticleAuthor(ArticlePageModel model) => 
            model.User is not null && model.User.Username == model.Article.Author.Username;

        private static bool CurrentUserIsCommentAuthor(ArticlePageModel model, Comment comment) => 
            comment.Author.Username == model.User?.Username;

        private static Node[] ShowFollowAndFavoriteButtons(ArticlePageModel model, Func<ArticlePageCommand, Task> dispatch)
        => [
                button([@class(["btn", "btn-sm", "btn-outline-secondary", "action-btn"]), type(["button"]), on.click(_ => dispatch(new FollowCommand()))], [
                    i([@class(["ion-plus-round"])], []),
                    text(model.Article.Author.Following ? " Unfollow" : " Follow" + $" {model.Article.Author.Username}")
                ]),
                text(" "),
                text(" "),
                button([@class(["btn", "btn-sm", "btn-outline-primary"]), type(["button"]), on.click(_ => dispatch(new InvertMarkArticleAsFavoriteCommand()))], [
                    i([@class(["ion-heart"])], []),
                    span([@class(["counter"])], [text($" Favorite Article ({model.Article.FavoritesCount})")]),
                ])
            ];

        private static Node[] ShowEditAndDeleteButtons(ArticlePageModel model, Func<ArticlePageCommand, Task> dispatch)
        => [
                i([@class(["btn", "btn-sm", "btn-outline-secondary"]), type(["button"]), on.click(_ => dispatch(new EditArticleCommand()))], [
                    i([@class(["ion-compose"])], []),
                    text(" Edit Article")
                ]),
                i([@class(["btn", "btn-sm", "btn-outline-danger"]), type(["button"]), on.click(_ => dispatch(new DeleteArticleCommand()))], [
                    i([@class(["ion-trash-a"])], []),
                    text(" Delete Article")
                ])];
    }




    public interface ArticlePageCommand;

    
    internal record DeleteCommentCommand(int Id) : ArticlePageCommand;
    internal record PostCommentCommand(string NewComment) : ArticlePageCommand;

    internal record SetNewComment(string Comment) : ArticlePageCommand;
    internal record EditArticleCommand : ArticlePageCommand;
    internal record DeleteArticleCommand : ArticlePageCommand;
    internal record InvertMarkArticleAsFavoriteCommand : ArticlePageCommand;
    internal record FollowCommand : ArticlePageCommand;

    public record ArticlePageModel
    {
        public Domain.Article Article { get; init; } = new Domain.Article("", "", "", "", [], DateTimeOffset.Now, DateTimeOffset.Now, false, 0, new Domain.Profile("", "", "", false));
        public User? User { get; init; } = null;
        public List<Comment> Comments { get; internal set; } = [];
        public string[] Errors { get; internal set; } = [];
        public string NewComment { get; internal set; } = ""; 
    }

}
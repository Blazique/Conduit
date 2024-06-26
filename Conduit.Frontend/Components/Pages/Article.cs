
using System.Diagnostics.Contracts;
using System.Security.Claims;
using Conduit.Domain;
using IdentityModel;
using k8s.KubeConfigModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
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
        public GetProfile GetProfile { get; set; } = (_) => Task.FromResult(None<Domain.Profile>());

        [Inject]
        public GetArticle GetArticle { get; set; } = (_) => Task.FromResult(Error<Domain.Article, string[]>(["Article not found"]));

        [Inject]
        public GetComments GetComments { get; set; } = (_) => Task.FromResult(new List<Comment>());

        [Inject]
        public AddComment AddComment { get; set; } = (_, _) => Task.FromResult(Error<Comment, string[]>(["Comment not added"]));

        [Inject]
        public DeleteComment DeleteComment {get;set;} = (_, _) => Task.FromResult(Error<Unit, string[]>(["Comment not deleted"]));

        [Inject]
        public MarkArticleAsFavorite MarkArticleAsFavorite { get; set; } = (_) => Task.CompletedTask;

        [Inject]
        public UnmarkArticleAsFavorite UnmarkArticleAsFavorite { get; set; } = (_) => Task.CompletedTask;

        [Inject]
        public FollowUser FollowUser { get; set; } = (_) => Task.FromResult(Error<Domain.Profile, string[]>(["User not followed"]));

        [Inject]
        public UnfollowUser UnfollowUser { get; set; } = (_) => Task.FromResult(Error<Domain.Profile, string[]>(["User not unfollowed"]));

        [Inject]
        public NavigationManager? Navigation { get; set; }

        

        [CascadingParameter]
        private Task<AuthenticationState>? AuthState { get; set; }

        private ClaimsPrincipal? _user;

        protected override async Task<ArticlePageModel> Initialize(ArticlePageModel model)
        {
            if (AuthState == null)
            {
                return model;
            }

            AuthenticationState authState = await AuthState;
            _user = authState.User;
            

            var article = await GetArticle((Slug)Slug);
            
            switch (article)
            {
                case Ok<Domain.Article, string[]>(var a):
                    var comments = new List<Comment>();
                    var profile = None<Domain.Profile>();
                    if (_user.Identity.IsAuthenticated)
                    {
                        model = model with { CurrentUserName = authState.User.Claims.Where(claim => claim.Type == "given_name").FirstOrDefault()?.Value, CurrentUserId = authState.User.Claims.Where(claim => claim.Type == "sub").FirstOrDefault()?.Value };
                        
                    }
                    profile = await GetProfile(a.Author.Username);
                    comments = await GetComments((Slug)a.Slug);

                    return profile switch
                    {
                        Some<Domain.Profile>(var p) => model with { Article = a, Comments = comments, Profile = p },
                        None<Domain.Profile> => model with { Article = a, Comments = comments },
                    };
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
                        await UnmarkArticleAsFavorite(model.Article.Slug);
                    }
                    else
                    {
                        await MarkArticleAsFavorite(model.Article.Slug);
                    }
                    
                    return model with {Article = model.Article with {Favorited = !model.Article.Favorited, FavoritesCount = model.Article.FavoritesCount + (model.Article.Favorited ? -1 : 1)}};
                case FollowCommand _:
                    Result<Domain.Profile, string[]> profile;
                    if (model.Article.Author.FollowedBy.Contains(model.CurrentUserId))
                    {
                        profile = await UnfollowUser(model.Article.Author.Username);
                        
                    }
                    else
                    {
                        profile = await FollowUser(model.Article.Author.Username);
                    }
                    return profile switch
                    {
                        Ok<Domain.Profile, string[]>(var p) => model with { Article = model.Article with { Author = p } },
                        Error<Domain.Profile, string[]>(var errors) => model with { Errors = errors }
                    };
                    
                case PostCommentCommand (var comment):
                    var addCommentResult = await AddComment((Slug)model.Article.Slug, comment);
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
                    var deleteCommentResult = await DeleteComment((Slug)model.Article.Slug, id);
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
                        model.Profile is not null && _user.Identity.IsAuthenticated
                        ?
                            div([@class(["row"])], [
                                div([@class(["col-xs-12", "col-md-8", "offset-md-2"])], [
                                    form([@class(["card", "comment-form"])], [
                                        div([@class(["card-block"])], [
                                                textarea([@class(["form-control"]), rows(["3"]), placeholder(["Write a comment..."]), on.input(async args => await dispatch(new SetNewComment(args.Value.ToString()))                                                  
                                                )], [])
                                            ]),
                                        div([@class(["card-footer"])], [
                                            img([src([model.Profile.Image]), @class(["comment-author-img"])], []),
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
                                    span([@class(["date-posted"])], [text(comment.CreatedAt.Year == DateTime.Now.Year ? DateTimeOffsetExtensions.FormatWithOrdinal(comment.CreatedAt) : DateTimeOffsetExtensions.FormatWithOrdinal(comment.CreatedAt) + " " + comment.CreatedAt.ToString("yyyy"))]),
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
            model.CurrentUserName == model.Article.Author.Username;

        private static bool CurrentUserIsCommentAuthor(ArticlePageModel model, Comment comment) =>
            comment.Author.Username == model.Profile.Username;

        private static Node[] ShowFollowAndFavoriteButtons(ArticlePageModel model, Func<ArticlePageCommand, Task> dispatch)
        => [
                button([@class(["btn", "btn-sm", "btn-outline-secondary", "action-btn"]), type(["button"]), on.click(_ => dispatch(new FollowCommand()))], [
                    i([@class(["ion-plus-round"])], []),
                    text(model.Article.Author.FollowedBy.Contains(model.CurrentUserId) ? " Unfollow" : " Follow" + $" {model.Article.Author.Username}")
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

    
    internal record DeleteCommentCommand(string Id) : ArticlePageCommand;
    internal record PostCommentCommand(string NewComment) : ArticlePageCommand;

    internal record SetNewComment(string Comment) : ArticlePageCommand;
    internal record EditArticleCommand : ArticlePageCommand;
    internal record DeleteArticleCommand : ArticlePageCommand;
    internal record InvertMarkArticleAsFavoriteCommand : ArticlePageCommand;
    internal record FollowCommand : ArticlePageCommand;

    public record ArticlePageModel
    {
        public Domain.Article Article { get; init; } = new Domain.Article("", "", "", "", [], DateTimeOffset.Now, DateTimeOffset.Now, new HashSet<string>(), false, 0, new Domain.Profile("", "", "", new HashSet<string>()));
        public List<Comment> Comments { get; internal set; } = [];
        public string[] Errors { get; internal set; } = [];
        public string NewComment { get; internal set; } = "";
        public Domain.Profile? Profile {get;  init;}
        public string? CurrentUserName { get; internal set; }
        public string? CurrentUserId { get; internal set; }
    }

}
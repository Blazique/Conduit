using Radix.Generators.Attributes;

using Radix.Data;

namespace Conduit.Domain;

[Alias<string>]
public readonly partial record struct Slug;

[Alias<string>]
public readonly partial record struct Tag;

public record Comment(string Id, string Body, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, Profile Author);
public record User(int Id, string Email, string Token, string Username, string Bio, string Image);

public record Profile(string Username, string Bio, string Image, HashSet<string> FollowedBy)
{
    public string Id { get; set; }
}

public record ArticleFeed(int ArticlesCount, List<Article> Articles);

public record Article(Slug Slug, string Title, string Description, string Body, Tag[] Tags, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, HashSet<string> FavoritedBy,  bool Favorited, int FavoritesCount, Profile Author);

public record UserLoggedIn(User User);


public delegate Task<Result<User, string>>? Login(string? email, string? password);

public delegate Task<Result<User, string[]>>? CreateUser(string? username, string? email, string? password);

public delegate Task<Option<User>> GetUser();

public delegate Task<Option<Profile>> GetProfile(string username);

public delegate Task<ArticleFeed> GetArticlesFeed(int? limit, int? offset);

public delegate Task<ArticleFeed> ListArticles(int? limit, int? offset, string? tag = null, string? author = null, string? favorited = null);

public delegate Task<string[]> GetTags();

public delegate Task MarkArticleAsFavorite(string slug);

public delegate Task UnmarkArticleAsFavorite(string slug);

public delegate Task<Result<Article, string[]>> GetArticle(Slug slug);

public delegate Task<Result<Article, string[]>> CreateArticle(string title, string description, string body, Tag[] tags);

public delegate Task<List<Comment>> GetComments(Slug slug);

public delegate Task<Result<Comment, string[]>> AddComment(Slug slug, string body);

public delegate Task<Result<Unit, string[]>> DeleteComment(Slug slug, string commentId);

public delegate Task<Result<Profile, string[]>> FollowUser(string username);

public delegate Task<Result<Profile, string[]>> UnfollowUser(string username);




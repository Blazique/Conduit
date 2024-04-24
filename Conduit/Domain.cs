using Conduit.ApiClient;

namespace Conduit.Domain;

public record User(int Id, string Email, string Token, string Username, string Bio, string Image);

public record Profile(string Username, string Bio, string Image, bool Following);

public record ArticleFeed(int ArticlesCount, List<Article> Articles);

public record Article(string Slug, string Title, string Description, string Body, List<string> TagList, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, bool Favorited, int FavoritesCount, Profile Author);

internal record UserLoggedIn(User User);

public static class DtoExtensions
{
    public static User ToUser(this ApiClient.User user) =>
        new(user.Id, user.Email, user.Token, user.Username, user.Bio, user.Image);

    public static Profile ToProfile(this ProfileDto profile) => 
        new(profile.Username, profile.Bio, profile.Image, profile.Following);

    public static Article ToArticle(this ArticleDto article) =>
        new(article.Slug, article.Title, article.Description, article.Body, article.TagList.ToList(), article.CreatedAt, article.UpdatedAt, article.Favorited, article.FavoritesCount, article.Author.ToProfile());
}

public delegate Task<Radix.Data.Result<User, string>>? Login(string? email, string? password);

public delegate Task<Radix.Data.Result<User, string[]>>? CreateUser(string? username, string? email, string? password);

public delegate Task<Radix.Data.Option<User>> GetUser();

public delegate Task<Radix.Data.Option<Profile>> GetProfile(string username);

public delegate Task<ArticleFeed> GetArticlesFeed(string token, int? limit, int? offset);

public delegate Task<ArticleFeed> GetAllRecentArticles(int? limit, int? offset);
using Conduit.Domain;

namespace Conduit.API;

public static class Extensions
{
    public static ProfileDto ToProfileDto(this Profile profile) =>
        new(profile.Id, profile.Username, profile.Bio, profile.Image, profile.Following);

    public static ArticleDto ToArticleDto(this Article article) =>
        new(article.Slug, article.Title, article.Description, article.Body, article.TagList, article.CreatedAt, article.UpdatedAt, article.FavoritedBy, article.Favorited, article.Author.ToProfileDto());

    public static CommentDto ToCommentDto(this Comment comment) =>
        new(comment.Id, comment.Body, comment.CreatedAt, comment.UpdatedAt, comment.Author.ToProfileDto());

    public static Article ToArticle(this ArticleDto article) =>
        new(article.Slug, article.Title, article.Description, article.Body, article.TagList, article.CreatedAt, article.UpdatedAt, article.FavoritedBy, article.Favorited, article.FavoritesCount, article.Author.ToProfile());

    public static Comment ToComment(this CommentDto comment) =>
        new(comment.Id, comment.Body, comment.CreatedAt, comment.UpdatedAt, comment.Author.ToProfile());

    public static ArticleFeed ToArticleFeed(this ArticlesDto articleFeed) =>
        new(articleFeed.ArticlesCount, articleFeed.Articles.Select(ToArticle).ToList());

    public static ArticlesDto ToArticleFeedDto(this ArticleFeed articleFeed) =>
        new(articleFeed.ArticlesCount, articleFeed.Articles.Select(ToArticleDto).ToList());

    public static Profile ToProfile(this ProfileDto profile) =>
        new(profile.Username, profile.Bio, profile.Image, profile.Following);
}
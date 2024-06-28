using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using static Radix.Control.Result.Extensions;
using static Radix.Control.Option.Extensions;
using Radix.Data;
using static Conduit.API.Extensions;
using Conduit.API;

namespace Conduit.Domain;

public static class Implementation
{
    public static Func<Client, GetProfile> getProfile =  (Client client) => async (string username) =>
    {
        var response = await client.GetProfileByUsernameAsync(username);
        return response is not null 
            ? Some(response.ToProfile()) 
            : None<Profile>();
    };

    public static Func<Client, ListArticles> listArticles = (Client client) => async (int? limit, int? offset, string? tag, string? author, string? favorited) =>
    {
        var response = await client.GetArticlesAsync(tag, author, favorited, limit, offset);
        return new ArticleFeed(response.ArticlesCount, response.Articles.Select(article => article.ToArticle()).ToList());
    };

    public static Func<Client, GetArticlesFeed> getArticlesFeed = (Client client) => async (int? limit, int? offset) =>
    {
        var response = await client.GetArticlesFeedAsync(limit, offset);
        return new ArticleFeed(response.ArticlesCount, response.Articles.Select(article => article.ToArticle()).ToList());
    };

    public static Func<Client, GetTags> getTags = (Client client) => async () =>
    {
        var response = await client.GetTagsAsync();
        return [.. response];
    };

    public static Func<Client, MarkArticleAsFavorite> markArticleAsFavorite = (Client client) => async (string slug) =>
    {
        await client.FavoriteArticleAsync(slug);
    };

    public static Func<Client, UnmarkArticleAsFavorite> unmarkArticleAsFavorite = (Client client) => async (string slug) =>
    {
        await client.UnfavoriteArticleAsync(slug);
    };

    public static Func<Client, GetArticle> getArticle = (Client client) => async (Slug slug) =>
    {

        var response = await client.GetArticleAsync(slug);
        return Ok<Article, string[]>(response.ToArticle());

    };

    public static Func<Client, GetComments> getComments = (Client client) => async (Slug slug) =>
    {
        var response = await client.GetArticleCommentsAsync(slug);
        return response.Select(comment => new Comment(comment.Id, comment.Body, comment.CreatedAt, comment.UpdatedAt, comment.Author.ToProfile())).ToList();
    };

    public static Func<Client, AddComment> addComment = (Client client) => async (Slug slug, string body) =>
    {

        var response = await client.AddCommentAsync(slug, body);
        return Ok<Comment, string[]>(response.ToComment());
        
    };

    public static Func<Client, DeleteComment> deleteComment = (Client client) => async (Slug slug, string commentId) =>
    {

        await client.DeleteCommentAsync(slug, commentId);
        return Ok<Unit, string[]>(new ());
        
    };

    public static Func<Client, FollowUser> followUser = (Client client) => async (string username) =>
    {
        var result = await client.FollowUserAsync(username);
        return Ok<Profile, string[]>(result.ToProfile());

    };

    public static Func<Client, UnfollowUser> unfollowUser = (Client client) => async (string username) =>
    {

        var result = await client.UnfollowUserAsync(username);
        return Ok<Profile, string[]>(result.ToProfile());

    };
}
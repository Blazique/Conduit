using Conduit.ApiClient;
using Conduit.Domain;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using static Radix.Control.Result.Extensions;
using static Radix.Control.Option.Extensions;
using Radix.Data;

namespace Conduit.Domain;

public static class Implementation
{
    public static Func<RealWorldClient, ProtectedSessionStorage, Login> loginUser = (RealWorldClient client, ProtectedSessionStorage sessionStorage) => async (string? email, string? password) =>
    {
        try
        {
            var response = await client.LoginAsync(new LoginUserRequest
            {
                User = new LoginUserDto
                {
                    Email = email,
                    Password = password
                }
            });
            
            await sessionStorage.SetAsync(LocalStorageKey.User, response.UserDto.ToUser());
            return Ok<Conduit.Domain.User, string>(response.UserDto.ToUser());
        }
        catch (Exception e)
        {
            return Error<Conduit.Domain.User, string>(e.Message);
        }
    };

    public static Func<RealWorldClient, CreateUser> createUser = (RealWorldClient client) => async (string? username, string? email, string? password) =>
    {
        try
        {
            var response = await client.CreateUserAsync(new NewUserRequest
            {
                User = new NewUser
                {
                    Username = username,
                    Email = email,
                    Password = password
                }
            });
            return Ok<Conduit.Domain.User, string[]>(response.UserDto.ToUser());
        }
        catch (ApiException<GenericErrorModel> e)
        {
            return Error<Conduit.Domain.User, string[]>(e.Result.Errors.Select(error => $"{error.Key}: {error.Value.Aggregate((s, s1) => s + ", and " + s1)}").ToArray());
        }
        catch (Exception e)
        {
            return Error<Conduit.Domain.User, string[]>([e.Message]);
        }
    };

    public static Func<ProtectedSessionStorage, GetUser> getUser = (ProtectedSessionStorage sessionStorage) => async () =>
    {
        var user = await sessionStorage.GetAsync<Conduit.Domain.User>(LocalStorageKey.User);
        return user.Value is not null ? Some(user.Value) : None<Conduit.Domain.User>();
    };

    public static Func<RealWorldClient, GetProfile> getProfile =  (RealWorldClient client) => async (string username) =>
    {
        var response = await client.GetProfileByUsernameAsync(username);
        return response is not null 
            ? Some(response.Profile.ToProfile()) 
            : None<Conduit.Domain.Profile>();
    };

    public static Func<RealWorldClient, GetAllRecentArticles> getAllRecentArticles =  (RealWorldClient client) => async (int? limit, int? offset, string? tag = null) =>
    {
        var response = await client.GetArticlesAsync(tag, null, null, limit, offset);
        return new ArticleFeed(response.ArticlesCount, response.Articles.Select(article => article.ToArticle()).ToList());
    };

    public static Func<RealWorldClient, GetArticlesFeed> getArticlesFeed = (RealWorldClient client) => async (string token, int? limit, int? offset) =>
    {
        client.SetAuthorizationHeader("Bearer", token);
        var response = await client.GetArticlesFeedAsync(limit, offset);
        return new ArticleFeed(response.ArticlesCount, response.Articles.Select(article => article.ToArticle()).ToList());
    };

    public static Func<RealWorldClient, GetTags> getTags = (RealWorldClient client) => async () =>
    {
        var response = await client.TagsAsync();
        return [.. response.Tags];
    };

    public static Func<RealWorldClient, MarkArticleAsFavorite> markArticleAsFavorite = (RealWorldClient client) => async (string slug, string token) =>
    {
        client.SetAuthorizationHeader("Bearer", token);
        await client.CreateArticleFavoriteAsync(slug);
    };

    public static Func<RealWorldClient, GetArticle> getArticle = (RealWorldClient client) => async (Slug slug) =>
    {
        try
        {
            var response = await client.GetArticleAsync(slug);
            return Ok<Article, string[]>(response.Article.ToArticle());
        }
        catch (ApiException<GenericErrorModel> e)
        {
            return Error<Article, string[]>(e.Result.Errors.Select(error => $"{error.Key}: ").ToArray());
        }
        catch (ApiException e)
        {
            return e.StatusCode switch
            {
                404 => Error<Article, string[]>(["Article not found"]),
                _ => Error<Article, string[]>([e.Message]),
            };
        }
    };

    public static Func<RealWorldClient, GetComments> getComments = (RealWorldClient client) => async (Slug slug) =>
    {
        var response = await client.GetArticleCommentsAsync(slug);
        return response.Comments.Select(comment => new Comment(comment.Id, comment.Body, comment.CreatedAt, comment.UpdatedAt, comment.Author.ToProfile())).ToList();
    };

    public static Func<RealWorldClient, AddComment> addComment = (RealWorldClient client) => async (Slug slug, string body, string token) =>
    {
        client.SetAuthorizationHeader("Bearer", token);
        
        try
        {
            var response = await client.CreateArticleCommentAsync(slug, new NewCommentRequest
            {
                Comment = new NewCommentDto
                {
                    Body = body
                }
            });
            return Ok<Comment, string[]>(response.Comment.ToComment());
        }
        catch (ApiException<GenericErrorModel> e)
        {
            return Error<Comment, string[]>(e.Result.Errors.Select(error => $"{error.Key}: ").ToArray());
        }
        catch (ApiException e)
        {
            return e.StatusCode switch
            {
                401 => Error<Comment, string[]>(["Not authorized to add a comment"]),
                _ => Error<Comment, string[]>([e.Message]),
            };
        }
        catch(Exception e)
        {
            return Error<Comment, string[]>([e.Message]);
        }
    };

    public static Func<RealWorldClient, DeleteComment> deleteComment = (RealWorldClient client) => async (Slug slug, int commentId, string token) =>
    {
        client.SetAuthorizationHeader("Bearer", token);
        
        try
        {
            await client.DeleteArticleCommentAsync(slug, commentId);
            return Ok<Unit, string[]>(new ());
        }
        catch (ApiException<GenericErrorModel> e)
        {
            return Error<Unit, string[]>(e.Result.Errors.Select(error => $"{error.Key}: ").ToArray());
        }
        catch (ApiException e)
        {
            return e.StatusCode switch
            {
                401 => Error<Unit, string[]>(["Not authorized to delete a comment"]),
                _ => Error<Unit, string[]>([e.Message]),
            };
        }
        catch(Exception e)
        {
            return Error<Unit, string[]>([e.Message]);
        }
    };
}
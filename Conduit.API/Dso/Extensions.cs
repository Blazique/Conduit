﻿namespace Conduit.API.Dso;

public static class Extensions
{
    public static string Id { get; private set; }

    public static ArticleDso ToDso(this ArticleDto article) => new ArticleDso { Slug = article.Slug, Title = article.Title, Description = article.Description, Body = article.Body, TagList = article.TagList, CreatedAt = article.CreatedAt, UpdatedAt = article.UpdatedAt, FavoritedBy = new HashSet<string>(), AuthorId = article.Author.Id, Comments = new List<CommentDso>() };


    public static ArticleDto ToDto(this ArticleDso article) => new ArticleDto(article.Slug, article.Title, article.Description, article.Body, article.TagList, article.CreatedAt, article.UpdatedAt, article.FavoritedBy, article.Favorited);

    public static CommentDso ToDso(this CommentDto comment) => new CommentDso(comment.Id, comment.Body, comment.CreatedAt, comment.UpdatedAt, comment.Author.ToDso());

    public static CommentDto ToDto(this CommentDso comment) =>  new CommentDto( comment.Body)
    {
        Author = comment.Author.ToDto(),
        CreatedAt = comment.CreatedAt,
        UpdatedAt = comment.UpdatedAt,
        Id = comment.Id
    };

    public static ProfileDso ToDso(this ProfileDto profile) => new ProfileDso { Id = profile.Id, Username = profile.Username, Bio = profile.Bio, Image = profile.Image, FollowedBy = profile.FollowedBy };

    public static ProfileDto ToDto(this ProfileDso profile) => new ProfileDto(profile.Id, profile.Username, profile.Bio, profile.Image, profile.FollowedBy);
}

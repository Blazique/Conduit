using Conduit.Domain;

namespace Conduit.API;

public record ArticleDto(string Slug, string Title, string Description, string Body, List<string> TagList, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, HashSet<string> FavoritedBy, bool Favorited)
{
    public int FavoritesCount => FavoritedBy.Count;
    public ProfileDto Author { get; init; }
}

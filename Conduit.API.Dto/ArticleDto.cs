using Conduit.Domain;

namespace Conduit.API;

public record ArticleDto(string Slug, string Title, string Description, string Body, List<string> TagList, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, HashSet<string> FavoritedBy, bool Favorited, ProfileDto Author)
{
    public int FavoritesCount => FavoritedBy.Count;
}

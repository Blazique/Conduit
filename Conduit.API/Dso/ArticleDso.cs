using Conduit.Domain;

namespace Conduit.API.Dso;

public record ArticleDso
{
    [Marten.Schema.Identity]
    public required string Slug { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Body { get; init; }
    public required List<string> TagList { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public HashSet<string> FavoritedBy { get; init; }
    public bool Favorited => FavoritedBy.Count > 0;
    public int FavoritesCount { get; init; }
    public required ProfileDso Author { get; init; }

    public required List<CommentDso> Comments { get; init; }
}

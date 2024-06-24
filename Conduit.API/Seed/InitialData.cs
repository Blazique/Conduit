using Marten.Schema;
using Marten;
using Conduit.API.Dso;

namespace Conduit.API.Seed;

public class InitialData : IInitialData
{
    private readonly object[] _initialData;

    public InitialData(params object[] initialData)
    {
        _initialData = initialData;
    }

    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        await using var session = store.LightweightSession();
        // Marten UPSERT will cater for existing records
        session.Store(_initialData);
        await session.SaveChangesAsync();
    }
}

public static class InitialDatasets
{
    public static readonly ProfileDso[] Profiles =
    {
        new ProfileDso{ Username = "Bob", Bio = "The World's Greatest Secret Agent", Image = "https://static.productionready.io/images/smiley-cyrus.jpg", Following = false },
        new ProfileDso{ Username = "Alice", Bio = "The Fastest Mouse in all of Mexico", Image = "https://static.productionready.io/images/smiley-cyrus.jpg", Following = false }
    };

    public static readonly ArticleDso[] Articles =
    {
        new ArticleDso{ Slug = "how-to-train-your-dragon", Title = "How to train your dragon", Description = "Ever wonder how?", Body = "You have to believe", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, TagList = new List<string>{ "dragons", "training" }, Favorited = true, FavoritesCount = 0, Author = Profiles[0] },
        new ArticleDso{ Slug = "the-quick-brown-fox", Title = "The quick brown fox", Description = "Jumped over the lazy dog", Body = "The quick brown fox jumped over the lazy dog", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, TagList = new List<string>{ "quick", "brown", "fox" }, Favorited = true, FavoritesCount = 0, Author = Profiles[1] }
    };

    public static readonly CommentDso[] Comments =
    {
        new CommentDso("1", "Great article!", DateTime.UtcNow, DateTime.UtcNow, Profiles[0]),
        new CommentDso("2", "Great article!", DateTime.UtcNow, DateTime.UtcNow, Profiles[1])
    };
}

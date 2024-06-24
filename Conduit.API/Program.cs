using Conduit;
using JasperFx.Core;
using Marten;
using Marten.Linq;
using Weasel.Core;

using Conduit.API.Seed;
using Conduit.API.Dso;
using Conduit.API;
using System.Security.Claims;

static string ToSlug(string title) => title.ToLower().Replace(" ", "-");

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<IdentityServerSettings>(builder.Configuration.GetSection(nameof(IdentityServerSettings)));
builder.Services.Configure<PostgresSettings>(builder.Configuration.GetSection(nameof(PostgresSettings)));

builder.AddServiceDefaults();
builder.AddNpgsqlDataSource(ConduitDatabase.Name);

var martenStoreOptions = new StoreOptions();
martenStoreOptions.Connection(builder.Configuration[PostgresSettingsConfigurationKeys.PostgresSettings_ConnectionString]);
martenStoreOptions.UseSystemTextJsonForSerialization();

// If we're running in development mode, let Marten just take care
// of all necessary schema building and patching behind the scenes
if (builder.Environment.IsDevelopment())
{
    martenStoreOptions.AutoCreateSchemaObjects = AutoCreate.All;
}

builder.Services
    .AddMarten(martenStoreOptions).UseLightweightSessions()
    .InitializeWith(
        new InitialData(InitialDatasets.Profiles), 
        new InitialData(InitialDatasets.Articles));

var app = builder.Build();

// Get a user's profile
app.MapGet("/profiles/{username}", (IDocumentSession session, string username) => {
    var profile = session.Load<ProfileDso>(username);
    if (profile is null) return Results.NotFound();
    return Results.Ok(profile.ToDto());
});

// Follow a user
app.MapPost("/profiles/{username}/follow", async (IDocumentSession session, string username) =>
{
    var profile = session.Load<ProfileDso>(username);
    if (profile is null) return Results.NotFound();
    session.Store(profile with { Following = true });
    await session.SaveChangesAsync();
    return Results.Ok(profile);
}).RequireAuthorization();

// Unfollow a user
app.MapDelete("/profiles/{username}/follow", async (IDocumentSession session, string username) =>
{
    var profile = session.Load<ProfileDso>(username);
    if (profile is null) return Results.NotFound();
    session.Store(profile with { Following = false });
    await session.SaveChangesAsync();
    return Results.Ok(profile);
}).RequireAuthorization();

app.MapGet("/articles/feed", async (IDocumentSession session, ClaimsPrincipal principal, int limit, int offset) =>
{
    var query = session.Query<ArticleDso>().Where(a => a.Author.Username == principal.Identity.Name);
    var articles = await query.Skip(offset).Take(limit).ToListAsync();
    return Results.Ok(articles.Select(dso => dso.ToDto()));
}).RequireAuthorization();

// Get a list of articles with optional filtering by tag and author and pagination
app.MapGet("/articles", async (IDocumentSession session, string? tag, string? author, int limit, int offset) =>
{

    var query = session
        .Query<ArticleDso>()
        .Where(a => tag == null || a.TagList.Contains(tag))
        .Where(a => author == null ||  a.Author.Username == author);
    var articles = await query.Skip(offset).Take(limit).ToListAsync();
    return Results.Ok(articles.Select(dso => dso.ToDto()));
});

// Get a single article by its slug
app.MapGet("/articles/{slug}", async (IDocumentSession session, string slug) =>
{
    var article = await session.LoadAsync<ArticleDso>(slug);
    if (article is null) return Results.NotFound();
    return Results.Ok(article.ToDto());
});

// Create a new article
app.MapPost("/articles", async (IDocumentSession session, ClaimsPrincipal principal, CreateArticleDto createArticleDto) =>
{
    var article = new ArticleDso
    {
        Author = await session.LoadAsync<ProfileDso>(principal.Identity.Name),
        Slug = ToSlug(createArticleDto.Title),
        Title = createArticleDto.Title,
        Description = createArticleDto.Description,
        Body = createArticleDto.Body,
        TagList = createArticleDto.TagList,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
        FavoritedBy = new HashSet<string>(),
        FavoritesCount = 0,
        Comments = new List<CommentDso>()
    };
    session.Store(article);
    await session.SaveChangesAsync();
    return Results.Created($"/articles/{article.Slug}", article.ToDto());
}).RequireAuthorization();

// Update an article
app.MapPut("/articles/{slug}", async (IDocumentSession session, string slug, UpdateArticleDto updateArticleDto) =>
{
    var article = await session.LoadAsync<ArticleDso>(slug);
    if (article is null) return Results.NotFound();
    var newSlug = updateArticleDto.Title is not null ? ToSlug(updateArticleDto.Title) : article.Slug;
    article = article with
    {
        Title = updateArticleDto.Title ?? article.Title,
        Description = updateArticleDto.Description ?? article.Description,
        Body = updateArticleDto.Body ?? article.Body,
        UpdatedAt = DateTimeOffset.UtcNow,
        Slug = newSlug
    };

    session.Store(article);
    await session.SaveChangesAsync();
    return Results.Ok(article.ToDto());
}).RequireAuthorization();

// Delete an article
app.MapDelete("/articles/{slug}", async (IDocumentSession session, string slug) =>
{
    var article = await session.LoadAsync<ArticleDso>(slug);
    if (article is null) return Results.NotFound();
    session.Delete(article);
    await session.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

// Add a comment to an article
app.MapPost("/articles/{slug}/comments", async (IDocumentSession session, string slug, CommentDto commentDto) =>
{
    var article = await session.LoadAsync<ArticleDso>(slug);
    if (article is null) return Results.NotFound();
    var comment = commentDto.ToDso();
    article.Comments.Add(comment);
    await session.SaveChangesAsync();
    return Results.Created($"/articles/{slug}/comments/{comment.Id}", comment.ToDto());
});

// Delete a comment from an article
app.MapDelete("/articles/{slug}/comments/{commentId}", async (IDocumentSession session, string slug, string commentId) =>
{
    var article = await session.LoadAsync<ArticleDso>(slug);
    if (article is null) return Results.NotFound();
    var comment = article.Comments.FirstOrDefault(c => c.Id == commentId);
    if (comment is null) return Results.NotFound();
    article.Comments.Remove(comment);
    await session.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

// Get a list of comments for an article
app.MapGet("/articles/{slug}/comments", async (IDocumentSession session, string slug) =>
{
    var article = await session.LoadAsync<ArticleDso>(slug);
    if (article is null) return Results.NotFound();
    return Results.Ok(article.Comments.Select(c => c.ToDto()));
}).RequireAuthorization();

// Favorite an article
app.MapPost("/articles/{slug}/favorite", async (IDocumentSession session, ClaimsPrincipal principal, string slug) =>
{
    // todo: check if the user is allowed to
    var article = await session.LoadAsync<ArticleDso>(slug);
    if (article is null) return Results.NotFound();
    article.FavoritedBy.Add(principal.Identity.Name);
    article = article with {  FavoritesCount = article.FavoritesCount + 1};
    await session.SaveChangesAsync();
    return Results.Ok(article.ToDto());
}).RequireAuthorization();

// Unfavorite an article
app.MapDelete("/articles/{slug}/favorite", async (IDocumentSession session, ClaimsPrincipal principal, string slug) =>
{
    // todo: check if the user is allowed to
    var article = await session.LoadAsync<ArticleDso>(slug);
    if (article is null) return Results.NotFound();
    article.FavoritedBy.Remove(principal.Identity.Name);
    article = article with { FavoritesCount = article.FavoritesCount - 1 };
    await session.SaveChangesAsync();
    return Results.Ok(article.ToDto());
}).RequireAuthorization();

app.MapGet("/tags", async (IDocumentSession session) =>
{
    var tags = await session.Query<ArticleDso>().SelectMany(a => a.TagList).Distinct().ToListAsync();
    return Results.Ok(tags);
});

app.Run();


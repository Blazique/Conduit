using Conduit;
using JasperFx.Core;
using Marten;
using Weasel.Core;

using Conduit.API.Seed;
using Conduit.API.Dso;
using Conduit.API;
using System.Security.Claims;

static string ToSlug(string title) => title.ToLower().Replace(" ", "-");

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<IdentityServerSettings>(builder.Configuration.GetSection(nameof(IdentityServerSettings)));
builder.Services.Configure<PostgresSettings>(builder.Configuration.GetSection(nameof(PostgresSettings)));

// ****************************************************************

// Configures Aspire defaults, service discovery, telemetry etc...
builder.AddServiceDefaults();

// Sets up the Aspire PostgreSQL component
builder.AddNpgsqlDataSource(ConduitDatabase.Name);

// ****************************************************************

var martenStoreOptions = new StoreOptions();
martenStoreOptions.UseSystemTextJsonForSerialization();

// If we're running in development mode, let Marten just take care
// of all necessary schema building and patching behind the scenes
if (builder.Environment.IsDevelopment())
{
    martenStoreOptions.AutoCreateSchemaObjects = AutoCreate.All;
}

var identityServerAuthority = builder.Configuration[IdentityServerSettingsConfigurationKeys.IdentityServerSettings_Authority];

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.Authority = identityServerAuthority;
        options.TokenValidationParameters.ValidateAudience = false;
    });
builder.Services.AddAuthorization();

builder.Services
    .AddMarten(martenStoreOptions)
    .UseLightweightSessions()
    .UseNpgsqlDataSource()
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
app.MapPost("/profiles/{username}/follow", async (IDocumentSession session, ClaimsPrincipal principal, string username) =>
{
    var nameIdentifier = principal.Claims.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
    var profile = session.Load<ProfileDso>(username);
    if (profile is null) return Results.NotFound();
    profile.FollowedBy.Add(nameIdentifier);
    session.Store(profile);
    await session.SaveChangesAsync();
    return Results.Ok(profile);
}).RequireAuthorization();

// Unfollow a user
app.MapDelete("/profiles/{username}/follow", async (IDocumentSession session, ClaimsPrincipal principal, string username) =>
{
    var nameIdentifier = principal.Claims.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
    var profile = session.Load<ProfileDso>(username);
    if (profile is null) return Results.NotFound();
    profile.FollowedBy.Remove(nameIdentifier);
    session.Store(profile);
    await session.SaveChangesAsync();
    return Results.Ok(profile);
}).RequireAuthorization();

app.MapGet("/articles/feed", async (IDocumentSession session, ClaimsPrincipal principal, int limit, int offset) =>
{
    var nameIdentifier = principal.Claims.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
    var query = session.Query<ArticleDso>().Where(a => a.AuthorId == nameIdentifier);
    var queriedArticles = await query.ToListAsync();
    var articlesDtos = await Task.WhenAll(queriedArticles.Skip(offset).Take(limit)
        .Select(async dso =>
        {
            var profile = await session.Query<ProfileDso>().FirstOrDefaultAsync(p => p.Id == dso.AuthorId);
            return dso.ToDto() with { Author = profile.ToDto() };
        }));
    var articles = new ArticlesDto(queriedArticles.Count, articlesDtos.ToList());
    return Results.Ok(articles);
}).RequireAuthorization();

// Get a list of articles with optional filtering by tag and author and pagination
app.MapGet("/articles", async (IDocumentSession session, string? tag, string? author, int limit, int offset, string favorited) =>
{
    var authorProfile = !string.IsNullOrEmpty(author) ? session.Query<ProfileDso>().FirstOrDefault(p => p.Username == author) : null;
    var authorProfileId = authorProfile?.Id;
    var favoritedByProfile = !string.IsNullOrEmpty(favorited) ? session.Query<ProfileDso>().FirstOrDefault(p => p.Username == favorited) : null;
    var favoritedByProfileId = favoritedByProfile?.Id;
    var query = session
        .Query<ArticleDso>()
        .Where(a => string.IsNullOrEmpty(tag) || a.TagList.Contains(tag))
        .Where(a => string.IsNullOrEmpty(author) || a.AuthorId == authorProfileId)
        .Where(a => string.IsNullOrEmpty(favorited) || a.FavoritedBy.Contains(favoritedByProfileId))
        .OrderBy(a => a.CreatedAt);
    var queriedArticles = await query.ToListAsync();
    var articlesDtos = await Task.WhenAll(queriedArticles.Skip(offset).Take(limit)
        .Select(async dso =>
        {
            var profile = await session.Query<ProfileDso>().FirstOrDefaultAsync(p => p.Id == dso.AuthorId);
            return dso.ToDto() with { Author = profile.ToDto() };
        }));
    var articles = new ArticlesDto(queriedArticles.Count, articlesDtos.ToList());
    return Results.Ok(articles);
});

// Get a single article by its slug
app.MapGet("/articles/{slug}", async (IDocumentSession session, string slug) =>
{
    var article = await session.LoadAsync<ArticleDso>(slug);
    var profile = await session.Query<ProfileDso>().FirstOrDefaultAsync(p => p.Id == article.AuthorId);
    if (article is null) return Results.NotFound();
    return Results.Ok(article.ToDto() with { Author = profile.ToDto()});
});

// Create a new article
app.MapPost("/articles", async (IDocumentSession session, ClaimsPrincipal principal, CreateArticleDto createArticleDto) =>
{
    var nameIdentifier = principal.Claims.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
    var article = new ArticleDso
    {
        AuthorId = nameIdentifier,
        Slug = ToSlug(createArticleDto.Title),
        Title = createArticleDto.Title,
        Description = createArticleDto.Description,
        Body = createArticleDto.Body,
        TagList = createArticleDto.TagList,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
        FavoritedBy = new HashSet<string>(),
        Comments = new List<CommentDso>()
    };
    var profile = await session.Query<ProfileDso>().FirstOrDefaultAsync(p => p.Id == nameIdentifier);
    session.Store(article);
    await session.SaveChangesAsync();
    return Results.Created($"/articles/{article.Slug}", article.ToDto() with { Author = profile.ToDto() });
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
app.MapPost("/articles/{slug}/comments", async (IDocumentSession session, ClaimsPrincipal principal, string slug, CommentDto commentDto) =>
{
    var nameIdentifier = principal.Claims.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
    var profile = await session.Query<ProfileDso>().FirstOrDefaultAsync(p => p.Id == nameIdentifier);
    var article = await session.LoadAsync<ArticleDso>(slug);
    if (article is null) return Results.NotFound();
    commentDto = commentDto with { Author = profile.ToDto() };
    var comment = commentDto.ToDso();
    comment = comment with { Id = Guid.NewGuid().ToString(), UpdatedAt = DateTimeOffset.UtcNow, CreatedAt = DateTimeOffset.Now };
    article.Comments.Add(comment);
    session.Store(article with { UpdatedAt = DateTimeOffset.UtcNow });
    await session.SaveChangesAsync();
    return Results.Created($"/articles/{slug}/comments/{comment.Id}", comment.ToDto());
}).RequireAuthorization();

// Delete a comment from an article
app.MapDelete("/articles/{slug}/comments/{commentId}", async (IDocumentSession session, string slug, string commentId) =>
{
    var article = await session.LoadAsync<ArticleDso>(slug);
    if (article is null) return Results.NotFound();
    var comment = article.Comments.FirstOrDefault(c => c.Id == commentId);
    if (comment is null) return Results.NotFound();
    article.Comments.Remove(comment);
    session.Store(article);
    await session.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

// Get a list of comments for an article
app.MapGet("/articles/{slug}/comments", async (IDocumentSession session, string slug) =>
{
    var article = await session.LoadAsync<ArticleDso>(slug);
    if (article is null) return Results.NotFound();
    return Results.Ok(article.Comments.Select(c => c.ToDto()));
});

// Favorite an article
app.MapPost("/articles/{slug}/favorite", async (IDocumentSession session, ClaimsPrincipal principal, string slug) =>
{
    // todo: check if the user is allowed to
    var article = await session.LoadAsync<ArticleDso>(slug);
    if (article is null) return Results.NotFound();
    var nameIdentifier = principal.Claims.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
    if (article.FavoritedBy.Add(nameIdentifier))
    {
        article = article with { UpdatedAt = DateTimeOffset.UtcNow };
        session.Store(article);
        await session.SaveChangesAsync();
    }
    return Results.Ok(article.ToDto());
}).RequireAuthorization();

// Unfavorite an article
app.MapDelete("/articles/{slug}/favorite", async (IDocumentSession session, ClaimsPrincipal principal, string slug) =>
{
// todo: check if the user is allowed to
var article = await session.LoadAsync<ArticleDso>(slug);
if (article is null) return Results.NotFound();
var nameIdentifier = principal.Claims.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
if (article.FavoritedBy.Remove(nameIdentifier))
{
    article = article with { UpdatedAt = DateTimeOffset.UtcNow };
    session.Store(article);
    await session.SaveChangesAsync();
}
return Results.Ok(article.ToDto());
}).RequireAuthorization();

app.MapGet("/tags", async (IDocumentSession session) =>
{
    var tags = await session.Query<ArticleDso>().SelectMany(a => a.TagList).Distinct().ToListAsync();
    return Results.Ok(tags);
});

app.UseAuthentication();
app.UseAuthorization();

app.Run();


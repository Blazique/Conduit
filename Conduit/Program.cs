using Conduit.ApiClient;
using Conduit.Components;
using Conduit.Domain;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Radix;
using static Radix.Control.Result.Extensions;
using static Radix.Control.Option.Extensions;
using Conduit;
using System.Net.Http.Headers;

var realWorldClient = new RealWorldClient("https://api.realworld.io/api/", new HttpClient());

Func<RealWorldClient, ProtectedSessionStorage, Conduit.Domain.Login> loginUser = (RealWorldClient client, ProtectedSessionStorage sessionStorage) => async (string? email, string? password) =>
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

Func<RealWorldClient, CreateUser> createUser = (RealWorldClient client) => async (string? username, string? email, string? password) =>
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
};

Func<ProtectedSessionStorage, GetUser> getUser = (ProtectedSessionStorage sessionStorage) => async () =>
{
    var user = await sessionStorage.GetAsync<Conduit.Domain.User>(LocalStorageKey.User);
    return user.Value is not null ? Some(user.Value) : None<Conduit.Domain.User>();
};

Func<RealWorldClient, GetProfile> getProfile =  (RealWorldClient client) => async (string username) =>
{
    try
    {
        var response = await client.GetProfileByUsernameAsync(username);
        return Some(response.Profile.ToProfile());
    }
    catch (Exception e)
    {
        return None<Conduit.Domain.Profile>();
    }
};

Func<RealWorldClient, GetAllRecentArticles> getAllRecentArticles =  (RealWorldClient client) => async (int? limit, int? offset) =>
{
    var response = await client.GetArticlesAsync(null, null, null, limit, offset);
    return new ArticleFeed(response.ArticlesCount, response.Articles.Select(article => article.ToArticle()).ToList());
};

Func<RealWorldClient, GetArticlesFeed> getArticlesFeed = (RealWorldClient client) => async (string token, int? limit, int? offset) =>
{
    client.SetAuthorizationHeader("Bearer", token);
    var response = await realWorldClient.GetArticlesFeedAsync(limit, offset);
    return new ArticleFeed(response.ArticlesCount, response.Articles.Select(article => article.ToArticle()).ToList());
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped(provider => loginUser(realWorldClient, provider.GetService<ProtectedSessionStorage>()));
builder.Services.AddSingleton((_) => createUser(realWorldClient));
builder.Services.AddScoped(provider => getUser(provider.GetService<ProtectedSessionStorage>()));
builder.Services.AddSingleton((_) => getProfile(realWorldClient));
builder.Services.AddSingleton((_) => getArticlesFeed(realWorldClient));
builder.Services.AddSingleton((_) => getAllRecentArticles(realWorldClient));
builder.Services.AddSingleton<MessageBus>();



var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();



app.Run();


public static class ServiceKeys
{
    public const string LoginUser = "loginUser";
}
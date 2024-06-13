using Conduit.ApiClient;
using Conduit.Components;
using static Conduit.Domain.Implementation;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Conduit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using static System.Net.WebRequestMethods;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var realWorldClient = new RealWorldClient("https://api.realworld.io/api/", new HttpClient());

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddHttpClient(OpenIdConnectDefaults.AuthenticationScheme, client =>
{
    // Use service discovery to resolve the address
    client.BaseAddress = new Uri($"https://{Conduit.IdentityServer.Name}");
});


builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        // Access the configuration system
        var configuration = builder.Configuration;

        // Read the "Identity__Url" environment variable
        var identityUrl = configuration["Identity:Url"];

        options.Authority = identityUrl;

        options.ClientId = Frontend.Name;

        options.ClientSecret = "secret";
        options.ResponseType = OpenIdConnectResponseType.Code;

        options.Scope.Clear();

        options.Scope.Add(OpenIdConnectScope.OpenIdProfile);
        options.Scope.Add(Conduit.Services.Access.Scopes.All.Name);
        options.Scope.Add(OpenIdConnectScope.OfflineAccess);
        options.Scope.Add("verification");
        options.ClaimActions.MapJsonKey("email_verified", "email_verified");
        options.GetClaimsFromUserInfoEndpoint = true;

        options.MapInboundClaims = false; // Don't rename claim types

        options.SaveTokens = true;
    });
    

builder.Services.AddScoped(provider => loginUser(realWorldClient, provider.GetService<ProtectedSessionStorage>()!));
builder.Services.AddSingleton((_) => createUser(realWorldClient));
builder.Services.AddScoped(provider => getUser(provider.GetService<ProtectedSessionStorage>()!));
builder.Services.AddSingleton((_) => getProfile(realWorldClient));
builder.Services.AddSingleton((_) => getArticlesFeed(realWorldClient));
builder.Services.AddSingleton((_) => getAllRecentArticles(realWorldClient));
builder.Services.AddSingleton((_) => markArticleAsFavorite(realWorldClient));
builder.Services.AddSingleton((_) => unmarkArticleAsFavorite(realWorldClient));
builder.Services.AddSingleton((_) => getTags(realWorldClient));
builder.Services.AddSingleton((_) => getArticle(realWorldClient));
builder.Services.AddSingleton((_) => getComments(realWorldClient));
builder.Services.AddSingleton((_) => addComment(realWorldClient));
builder.Services.AddSingleton((_) => deleteComment(realWorldClient));
builder.Services.AddSingleton((_) => followUser(realWorldClient));
builder.Services.AddSingleton((_) => unfollowUser(realWorldClient));
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

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();

// Add security headers
app.Use(async (context, next) =>
{
    
    // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referrer-Policy
    var referrer_policy = "no-referrer";
    if (!context.Response.Headers.ContainsKey("Referrer-Policy"))
    {
        context.Response.Headers.Append("Referrer-Policy", referrer_policy);
    }

    // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy
    var csp = "default-src 'self'; object-src 'none'; style-src 'self'  https://demo.productionready.io https://code.ionicframework.com ;font-src https://fonts.googleapis.com https://code.ionicframework.com; img-src https://api.realworld.io; frame-ancestors 'none'; sandbox allow-forms allow-same-origin allow-scripts; base-uri 'self'; upgrade-insecure-requests;";

    // once for standards compliant browsers
    if (!context.Response.Headers.ContainsKey("Content-Security-Policy"))
    {
        context.Response.Headers.Append("Content-Security-Policy", csp);
    }
    // and once again for IE
    if (!context.Response.Headers.ContainsKey("X-Content-Security-Policy"))
    {
        context.Response.Headers.Append("X-Content-Security-Policy", csp);
    }



    await next();
});

app.Run();
using Conduit.ApiClient;
using Conduit.Components;
using static Conduit.Domain.Implementation;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Conduit;

var realWorldClient = new RealWorldClient("https://api.realworld.io/api/", new HttpClient());

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped(provider => loginUser(realWorldClient, provider.GetService<ProtectedSessionStorage>()!));
builder.Services.AddSingleton((_) => createUser(realWorldClient));
builder.Services.AddScoped(provider => getUser(provider.GetService<ProtectedSessionStorage>()!));
builder.Services.AddSingleton((_) => getProfile(realWorldClient));
builder.Services.AddSingleton((_) => getArticlesFeed(realWorldClient));
builder.Services.AddSingleton((_) => getAllRecentArticles(realWorldClient));
builder.Services.AddSingleton((_) => markArticleAsFavorite(realWorldClient));
builder.Services.AddSingleton((_) => getTags(realWorldClient));
builder.Services.AddSingleton((_) => getArticle(realWorldClient));
builder.Services.AddSingleton((_) => getComments(realWorldClient));
builder.Services.AddSingleton((_) => addComment(realWorldClient));
builder.Services.AddSingleton((_) => deleteComment(realWorldClient));
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
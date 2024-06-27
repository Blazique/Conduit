using Conduit.Domain;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Conduit.API;

public class Client
{
    private readonly HttpClient _httpClient;

    public Client(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ArticlesDto> GetArticlesFeedAsync(int? limit, int? offset)
    {
        return await _httpClient.GetFromJsonAsync<ArticlesDto>($"/articles/feed?limit={limit}&offset={offset}");
    }

    public async Task<ArticlesDto> GetArticlesAsync(string? tag, string? author, bool? favorited, int? limit, int? offset)
    {
        return await _httpClient.GetFromJsonAsync<ArticlesDto>($"/articles?limit={limit}&offset={offset}&tag={tag}&author={author}");
    }

    public async Task<ArticleDto> GetArticleAsync(string slug)
    {
        return await _httpClient.GetFromJsonAsync<ArticleDto>($"/articles/{slug}");
    }

    public async Task<ArticleDto> CreateArticleAsync(CreateArticleDto createArticleDto)
    {
        var response = await _httpClient.PostAsJsonAsync("/articles", createArticleDto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ArticleDto>();
    }

    public async Task<ArticleDto> UpdateArticleAsync(string id, UpdateArticleDto updateArticleDto)
    {
        var response = await _httpClient.PutAsJsonAsync($"/articles/{id}", updateArticleDto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ArticleDto>();
    }

    public async Task DeleteArticleAsync(string id)
    {
        var response = await _httpClient.DeleteAsync("$/articles/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<ProfileDto> GetProfileByUsernameAsync(string userName)
    {
        return await _httpClient.GetFromJsonAsync<ProfileDto>($"/profiles/{userName}");
    }

    public async Task<ProfileDto> FollowUserAsync(string userName)
    {
        var response = await _httpClient.PostAsync($"/profiles/{userName}/follow", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProfileDto>();
    }

    public async Task<ProfileDto> UnfollowUserAsync(string userName)
    {
        var response = await _httpClient.DeleteAsync($"/profiles/{userName}/follow");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProfileDto>();
    }

    public async Task<CommentDto> AddCommentAsync(string slug, string body)
    {
        var response = await _httpClient.PostAsJsonAsync($"/articles/{slug}/comments", new CommentDto(body));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommentDto>();
    }

    public async Task DeleteCommentAsync(string slug, string commentId)
    {
        var response = await _httpClient.DeleteAsync($"/articles/{slug}/comments/{commentId}");
        response.EnsureSuccessStatusCode();
    }

   public async Task<List<CommentDto>> GetArticleCommentsAsync(Slug slug)
    {
        return await _httpClient.GetFromJsonAsync<List<CommentDto>>($"/articles/{slug}/comments");
    }

    public async Task<List<string>> GetTagsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<string>>("/tags");
    }

    public async Task<ArticleDto> FavoriteArticleAsync(string slug)
    {
        var response = await _httpClient.PostAsync($"/articles/{slug}/favorite", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ArticleDto>();
    }

    public async Task<ArticleDto> UnfavoriteArticleAsync(string slug)
    {
        var response = await _httpClient.DeleteAsync($"/articles/{slug}/favorite");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ArticleDto>();
    }

}
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

    public Client(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public async Task<ArticleFeedDto> GetArticlesFeedAsync(int? limit, int? offset)
    {
        return await _httpClient.GetFromJsonAsync<ArticleFeedDto>($"/api/articles/feed?limit={limit}&offset={offset}");
    }

    public async Task<ArticleFeedDto> GetArticlesAsync(string? tag, string? author, bool? favorited, int? limit, int? offset)
    {
        return await _httpClient.GetFromJsonAsync<ArticleFeedDto>($"/api/articles?limit={limit}&offset={offset}&tag={tag}&author={author}");
    }

    public async Task<ArticleDto> GetArticleAsync(string slug)
    {
        return await _httpClient.GetFromJsonAsync<ArticleDto>($"/api/articles/{slug}");
    }

    public async Task<ArticleDto> CreateArticleAsync(CreateArticleDto createArticleDto)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/articles", createArticleDto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ArticleDto>();
    }

    public async Task<ArticleDto> UpdateArticleAsync(string id, UpdateArticleDto updateArticleDto)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/articles/{id}", updateArticleDto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ArticleDto>();
    }

    public async Task DeleteArticleAsync(string id)
    {
        var response = await _httpClient.DeleteAsync($"/api/articles/{id}");
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
        var response = await _httpClient.PostAsJsonAsync($"/api/articles/{slug}/comments", new { comment = new { body } });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommentDto>();
    }

    public async Task DeleteCommentAsync(string slug, int commentId)
    {
        var response = await _httpClient.DeleteAsync($"/api/articles/{slug}/comments/{commentId}");
        response.EnsureSuccessStatusCode();
    }

   public async Task<List<CommentDto>> GetArticleCommentsAsync(Slug slug)
    {
        return await _httpClient.GetFromJsonAsync<List<CommentDto>>($"/api/articles/{slug}/comments");
    }

    public async Task<List<string>> GetTagsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<string>>("/api/tags");
    }

    public async Task<ArticleDto> FavoriteArticleAsync(string slug)
    {
        var response = await _httpClient.PostAsync($"/api/articles/{slug}/favorite", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ArticleDto>();
    }

    public async Task<ArticleDto> UnfavoriteArticleAsync(string slug)
    {
        var response = await _httpClient.DeleteAsync($"/api/articles/{slug}/favorite");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ArticleDto>();
    }

}
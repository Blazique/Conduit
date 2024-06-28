using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace Conduit.Frontend;

/// <summary>
/// Adds the access token to the Authorization header of the request.
/// </summary>
public class TokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
        return await base.SendAsync(request, cancellationToken);
    }
}

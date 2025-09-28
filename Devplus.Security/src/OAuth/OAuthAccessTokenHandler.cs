using Devplus.Security.OAuth.Contracts;
using Devplus.Security.OAuth.Refit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Security.OAuth;

public class OAuthAccessTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _http;
    private readonly IDevplusOAuthService _refit;
    private readonly IOptions<OAuthSettings> _opt;

    public OAuthAccessTokenHandler(
        IHttpContextAccessor http,
        IDevplusOAuthService refit,
        IOptions<OAuthSettings> opt)
    {
        _http = http;
        _refit = refit;
        _opt = opt;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var s = _opt.Value;
        var tokenReq = new TokenRequestDto
        {
            GrantType = "client_credentials",
            ClientId = Guid.Parse(s.ClientId ?? throw new InvalidOperationException("OAuthSettings:ClientId ausente")),
            ClientSecret = s.ClientSecret ?? throw new InvalidOperationException("OAuthSettings:ClientSecret ausente")
        };

        var tokenRes = await _refit.GetTokenAsync(tokenReq);
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenRes.AccessToken);

        return await base.SendAsync(request, ct);
    }
}

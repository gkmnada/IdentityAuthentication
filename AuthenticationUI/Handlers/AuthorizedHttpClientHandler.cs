using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Security.Claims;
using AuthenticationUI.Models;
using System.Text.Json;

namespace AuthenticationUI.Handlers
{
    public class AuthorizedHttpClientHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;

        public AuthorizedHttpClientHandler(IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _clientFactory = clientFactory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "JwtAuthToken")?.Value;
            var refreshToken = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "RefreshToken")?.Value;

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !string.IsNullOrWhiteSpace(refreshToken))
            {
                var newTokens = await RefreshToken(refreshToken);
                if (newTokens != null)
                {
                    var claims = new List<Claim>
                {
                    new Claim("JwtAuthToken", newTokens.Token),
                    new Claim("RefreshToken", newTokens.RefreshToken)
                };

                    var claimsIdentity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = false
                    };

                    await _httpContextAccessor.HttpContext.SignInAsync(JwtBearerDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newTokens.Token);
                    response = await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    await _httpContextAccessor.HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
                    _httpContextAccessor.HttpContext.Response.Redirect("/Login/Index");
                }
            }

            return response;
        }

        private async Task<JwtResponseModel> RefreshToken(string refreshToken)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7229/api/Token?refreshToken=" + refreshToken);
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                return System.Text.Json.JsonSerializer.Deserialize<JwtResponseModel>(jsonData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            return null;
        }
    }
}

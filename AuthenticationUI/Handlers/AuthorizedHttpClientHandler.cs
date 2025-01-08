using AuthenticationUI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
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
            var accessToken = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "IdentityAuthentication")?.Value;
            var refreshToken = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "RefreshToken")?.Value;

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !string.IsNullOrWhiteSpace(refreshToken))
            {
                var newToken = await RefreshToken(refreshToken);

                if (newToken != null)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var token = tokenHandler.ReadJwtToken(newToken.AccessToken);

                    var claims = _httpContextAccessor.HttpContext.User.Claims.ToList();

                    claims.RemoveAll(x => x.Type == "IdentityAuthentication" || x.Type == "RefreshToken");

                    claims.Add(new Claim("IdentityAuthentication", newToken.AccessToken));
                    claims.Add(new Claim("RefreshToken", newToken.RefreshToken));

                    var roles = token.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();

                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true
                    };

                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

                    _httpContextAccessor.HttpContext.User = claimsPrincipal;

                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken.AccessToken);
                    response = await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    _httpContextAccessor.HttpContext.Response.Redirect("/Login/Index");
                    await _httpContextAccessor.HttpContext.Response.CompleteAsync();
                }
            }
            return response;
        }

        private async Task<JwtResponseModel> RefreshToken(string refreshToken)
        {
            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = refreshToken
            };

            var client = _clientFactory.CreateClient();

            var json = JsonConvert.SerializeObject(refreshTokenDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync("https://localhost:7229/api/Token", content);

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

        #region DTO
        private class RefreshTokenDto
        {
            public string RefreshToken { get; set; }
        }
        #endregion
    }
}

using AuthenticationAPI.Tools;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthenticationAPI.Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var tokenDefaults = new JwtTokenDefaults();

            configuration.Bind("JwtTokenDefaults", tokenDefaults);
            services.Configure<JwtTokenDefaults>(configuration.GetSection("JwtTokenDefaults"));

            var key = Encoding.UTF8.GetBytes(tokenDefaults.SecretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = tokenDefaults.ValidIssuer,
                        ValidAudience = tokenDefaults.ValidAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                    options.MapInboundClaims = false;
                });

            services.AddScoped<JwtTokenGenerator>();

            return services;
        }
    }
}

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Middleware;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing");
        var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing");
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is missing");
        var clockSkewMinutes = int.TryParse(jwtSection["ClockSkewMinutes"], out var skew) ? skew : 0;

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(clockSkewMinutes)
                };

                // Set up blacklist check on token validation
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var jwtService = context.HttpContext.RequestServices.GetRequiredService<IJwtTokenService>();

                        // Expect a custom claim "tokenId" (int) embedded when issuing tokens
                        var tokenIdClaim = context.Principal?.FindFirst("tokenId")?.Value;
                        if (string.IsNullOrWhiteSpace(tokenIdClaim) || !int.TryParse(tokenIdClaim, out var tokenId))
                        {
                            context.Fail("Invalid token: tokenId claim missing or invalid");
                            return;
                        }

                        var isBlacklisted = await jwtService.IsTokenBlacklisted(tokenId);
                        if (isBlacklisted)
                        {
                            context.Fail("Token is blacklisted");
                        }
                    }
                };
            });

        return services;
    }
}

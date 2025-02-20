#region

using System.Text;
using Microsoft.IdentityModel.Tokens;
using zora.Core;

#endregion

namespace zora.Extensions;

public static class TokenValidationExtensions
{
    private static string? _cachedIssuer;
    private static string? _cachedAudience;

    public static string GetIssuer(IWebHostEnvironment environment)
    {
        return TokenValidationExtensions._cachedIssuer ??= environment.IsDevelopment()
            ? "https://localhost:5001"
            : Constants.ZORA_URL;
    }

    public static string GetAudience(IWebHostEnvironment environment)
    {
        return TokenValidationExtensions._cachedAudience ??= environment.IsDevelopment()
            ? "https://localhost:4200"
            : Constants.ZORA_URL;
    }

    public static TokenValidationParameters CreateTokenValidationParameters(string issuerSigningKey)
    {
        byte[] key = Encoding.UTF8.GetBytes(issuerSigningKey);
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudiences =
            [
                Constants.ZORA_URL, Constants.ZORA_SUBDOMAIN_URL, Constants.ZORA_URL_WITH_PORT, "https://localhost:4200"
            ],
            ValidIssuers =
            [
                Constants.ZORA_URL, Constants.ZORA_SUBDOMAIN_URL, Constants.ZORA_URL_WITH_PORT, "https://localhost:5001"
            ]
        };
    }
}

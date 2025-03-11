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
        return _cachedIssuer ??= environment.IsDevelopment()
            ? Constants.LOCAL_API_URL
            : Constants.ZORA_URL;
    }

    public static string GetAudience(IWebHostEnvironment environment)
    {
        return _cachedAudience ??= environment.IsDevelopment()
            ? Constants.LOCAL_CLIENT_URL
            : Constants.ZORA_SUBDOMAIN_URL;
    }

    public static TokenValidationParameters CreateTokenValidationParameters(string issuerSigningKey)
    {
        byte[] key = Encoding.UTF8.GetBytes(issuerSigningKey);

        string[] validAudiences =
        [
            Constants.ZORA_SUBDOMAIN_URL,
            Constants.LOCAL_CLIENT_URL
        ];

        string[] validIssuers =
        [
            Constants.ZORA_URL,
            Constants.LOCAL_API_URL
        ];

        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudiences = validAudiences,
            ValidIssuers = validIssuers,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }
}

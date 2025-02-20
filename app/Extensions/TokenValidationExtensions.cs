#region

using System.Text;
using Microsoft.IdentityModel.Tokens;
using zora.Core;

#endregion

namespace zora.Extensions;

public static class TokenValidationExtensions
{
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
                Constants.ZORA_URL,
                Constants.ZORA_SUBDOMAIN_URL,
                Constants.ZORA_URL_WITH_PORT,
                "https://localhost:4200"
            ],
            ValidIssuers =
            [
                Constants.ZORA_URL,
                Constants.ZORA_SUBDOMAIN_URL,
                Constants.ZORA_URL_WITH_PORT,
                "https://localhost:5001"
            ]
        };
    }
}

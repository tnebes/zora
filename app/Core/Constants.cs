namespace zora.Core;

public static class Constants
{
    public const string ISSUER_SIGNING_KEY = "Zora:SecretKey";
    public const string ERROR_500_MESSAGE = "Something went wrong on our side.";
    public const string ERROR_404_MESSAGE = "Not found.";
    public const string ZORA_URL = "https://draucode.com";
    public const string ZORA_URL_WITH_PORT = $"{ZORA_URL}:5000";
    public const string ZORA_SUBDOMAIN = "zora";
    public const string ZORA_SUBDOMAIN_URL = $"https://{ZORA_SUBDOMAIN}.{ZORA_URL}";
    public const string CONNECTION_STRING_KEY = "Zora:ConnectionString";
    public const string ZORA_CORS_POLICY_NAME = "ZoraCorsPolicy";
    public const string ADMIN = "Admin";
    public const string ERROR_TYPE = "errorType";
    public const int TOKEN_EXPIRATION_HOURS = 24;
    public const int MAX_RESULTS_PER_PAGE = 1000;
    public const int DEFAULT_PAGE_SIZE = 50;
}

namespace zora.Core;

public static class Constants
{
    public const string ISSUER_SIGNING_KEY = "Zora:SecretKey";
    public const string ERROR_500_MESSAGE = "Something went wrong on our side.";
    public const string ERROR_404_MESSAGE = "Not found.";
    public const string DRAUCODE = "draucode";
    public const string DRAUCODE_COM = $"{Constants.DRAUCODE}.com";
    public const string ZORA_URL = $"https://{Constants.DRAUCODE_COM}";
    public const string ZORA_URL_WITH_PORT = $"{Constants.ZORA_URL}:5000";
    public const string ZORA_SUBDOMAIN = "zora";
    public const string ZORA_SUBDOMAIN_URL = $"https://{Constants.ZORA_SUBDOMAIN}.{Constants.DRAUCODE_COM}";
    public const string CONNECTION_STRING_KEY = "Zora:ConnectionString";
    public const string ZORA_CORS_POLICY_NAME = "ZoraCorsPolicy";
    public const string ADMIN = "Admin";
    public const string ERROR_TYPE = "errorType";
    public const int TOKEN_EXPIRATION_HOURS = 24;
    public const int MAX_RESULTS_PER_PAGE = 1000;
    public const int DEFAULT_PAGE_SIZE = 50;
    public const string WWW_ROOT = "wwwroot";
    public const string CONTENT = "content";
    public const string ASSETS = "assets";
    public const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
    public static readonly string[] ALLOWED_FILE_EXTENSIONS = { ".jpg", ".jpeg", ".png", ".gif", ".pdf" };
}

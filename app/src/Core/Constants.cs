namespace zora.Core;

public static class Constants
{
    public const string ISSUER_SIGNING_KEY = "Zora:SecretKey";
    public const string ERROR_500_MESSAGE = "Something went wrong on our side.";
    public const string ERROR_404_MESSAGE = "Not found.";
    public const string DRAUCODE = "draucode";
    public const string DRAUCODE_COM = $"{DRAUCODE}.com";
    public const string ZORA_URL = $"https://{DRAUCODE_COM}";
    public const string ZORA_URL_WITH_PORT = $"{ZORA_URL}:5000";
    public const string ZORA_SUBDOMAIN = "zora";
    public const string ZORA_SUBDOMAIN_URL = $"https://{ZORA_SUBDOMAIN}.{DRAUCODE_COM}";
    public const string CONNECTION_STRING_KEY = "Zora:ConnectionString";
    public const string ZORA_CORS_POLICY_NAME = "ZoraCorsPolicy";
    public const string ADMIN = "Admin";
    public const string ERROR_TYPE = "errorType";
    public const int TOKEN_EXPIRATION_HOURS = 24;
    public const string JWT_ISSUER = "Jwt:Issuer";
    public const string JWT_AUDIENCE = "Jwt:Audience";
    public const int MAX_RESULTS_PER_PAGE = 1000;
    public const int DEFAULT_PAGE_SIZE = 50;
    public const int MAX_FILE_SIZE_MB = 10;
    public const long MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024;
    public const string PRODUCTION_BASE_PATH = "/var/www/zora";
    public const string WWW_ROOT = "wwwroot";
    public const string LOCAL_API_URL = "https://localhost:5001";
    public const string LOCAL_CLIENT_URL = "https://localhost:4200";
    public const string ASSETS_FOLDER = "assets";
    public const string CONTENT_FOLDER = "content";
    public static readonly string[] ALLOWED_FILE_EXTENSIONS = [".jpg", ".jpeg", ".png", ".gif", ".pdf"];
    public static readonly string REASON = "reason";
    public static readonly string CONTENT_ASSETS_FOLDER = ASSETS_FOLDER + Path.DirectorySeparatorChar + CONTENT_FOLDER;
}

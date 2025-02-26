namespace zora.Core.DTOs;

public sealed class AuthenticationStatusDto
{
    public bool IsAuthenticated { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

namespace zora.Core.DTOs;

public sealed class TokenResponseDto
{
    public string Token { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}

namespace zora.Core.DTOs.Responses;

public sealed class TokenResponseDto
{
    public string Token { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}

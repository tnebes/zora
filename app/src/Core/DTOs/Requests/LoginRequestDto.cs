namespace zora.Core.DTOs.Requests;

public sealed class LoginRequestDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? Token { get; set; }
}

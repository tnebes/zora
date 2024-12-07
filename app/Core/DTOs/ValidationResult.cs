namespace zora.Core.DTOs;

public sealed class ValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public int StatusCode { get; set; }
}

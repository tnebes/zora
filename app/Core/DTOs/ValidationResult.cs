namespace zora.Core.DTOs;

public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int StatusCode { get; private set; }

    public static ValidationResult Success() =>
        new() { IsValid = true };

    public static ValidationResult Fail(string message, int statusCode) =>
        new() { IsValid = false, ErrorMessage = message, StatusCode = statusCode };
}

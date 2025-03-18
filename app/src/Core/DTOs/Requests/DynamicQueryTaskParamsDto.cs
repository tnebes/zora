namespace zora.Core.DTOs.Requests;

public sealed class DynamicQueryTaskParamsDto : DynamicQueryParamsDto
{
    protected override Dictionary<string, string> GetParameters() => throw new NotImplementedException();
}

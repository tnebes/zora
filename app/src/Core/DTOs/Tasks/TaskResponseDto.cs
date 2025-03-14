#region

using zora.Core.DTOs.Responses.Interface;

#endregion

namespace zora.Core.DTOs.Tasks;

public sealed class TaskResponseDto : IResponseDto<ReadTaskDto>
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public IEnumerable<ReadTaskDto> Items { get; set; }
}

namespace zora.Core.DTOs.Interface;

public interface IResponseDto<T>
{
    public IEnumerable<T> Items { get; set; }
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

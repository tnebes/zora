namespace zora.Core.DTOs.Responses.Interface;

public interface IResponseDto<T>
{
    public int Total { get; set; } // total number of queries that match the search criteria
    public int Page { get; set; }
    public int PageSize { get; set; }
    public IEnumerable<T> Items { get; set; }
}

using FluentResults;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;

namespace zora.Core.Interfaces.Services;

public interface IBaseService<TEntity, TCreateDto, TUpdateDto, TResponseDto>
    where TEntity : class
    where TResponseDto : class
{
    Task<Result<(IEnumerable<TEntity>, int total)>> GetAsync(QueryParamsDto queryParams);
    Task<Result<TResponseDto>> GetDtoAsync(QueryParamsDto queryParams);
    Task<Result<TEntity>> GetByIdAsync(long id);
    Task<Result<TEntity>> CreateAsync(TCreateDto createDto);
    Task<Result<TEntity>> UpdateAsync(long id, TUpdateDto updateDto);
    Task<bool> DeleteAsync(long id);
    Task<Result<TResponseDto>> FindAsync(QueryParamsDto findParams);
    Task<Result<TResponseDto>> SearchAsync(DynamicQueryParamsDto searchParams);
}

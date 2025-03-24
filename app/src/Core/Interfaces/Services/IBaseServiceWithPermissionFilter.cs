#region

using FluentResults;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.Core.Interfaces.Services;

public interface IBaseServiceWithPermissionFilter<TEntity, TCreateDto, TUpdateDto, TResponseDto, TDynamicQueryParamsDto>
    where TEntity : class
    where TResponseDto : class
    where TDynamicQueryParamsDto : DynamicQueryParamsDto
{
    Task<Result<(IEnumerable<TEntity>, int total)>> GetAsync(QueryParamsDto queryParams, long userId);
    Task<Result<TResponseDto>> GetDtoAsync(QueryParamsDto queryParams, long userId);
    Task<Result<TEntity>> GetByIdAsync(long id, long userId, bool includeProperties = false);
    Task<Result<TEntity>> CreateAsync(TCreateDto createDto, long userId);
    Task<Result<TEntity>> UpdateAsync(long id, TUpdateDto updateDto, long userId);
    Task<bool> DeleteAsync(long id, long userId);
    Task<Result<TResponseDto>> FindAsync(QueryParamsDto findParams, long userId);
    Task<Result<TResponseDto>> SearchAsync(TDynamicQueryParamsDto searchParams, long userId);
}

#region

using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface ISearchRepository<TEntity, TDynamicQueryParamsDto> where TEntity : BaseEntity
    where TDynamicQueryParamsDto : DynamicQueryParamsDto
{
    Task<Result<(IEnumerable<TEntity>, int TotalCount)>> SearchAsync(TDynamicQueryParamsDto searchParams,
        bool includeProperties = false);
}

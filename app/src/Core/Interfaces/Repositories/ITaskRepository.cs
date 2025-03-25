#region

using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface ITaskRepository : ISearchRepository<ZoraTask, DynamicQueryTaskParamsDto>
{
    Task<Result<(IEnumerable<ZoraTask>, int total)>> GetPagedAsync(IQueryable<ZoraTask> query, int page, int pageSize);
    IQueryable<ZoraTask> GetQueryable();
    Task<Result<ZoraTask>> GetByIdAsync(long id, bool includeProperties);
    Task<Result<ZoraTask>> UpdateAsync(ZoraTask task);
    Task<Result<bool>> DeleteAsync(ZoraTask task);
    Task<Result<ZoraTask>> CreateAsync(ZoraTask task);
}

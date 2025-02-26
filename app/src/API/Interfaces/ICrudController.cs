#region

using Microsoft.AspNetCore.Mvc;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.API.Interfaces;

public interface ICrudController<TEntity, TCreateDto, TUpdateDto, TResponseDto, TDynamicQueryDto>
    where TDynamicQueryDto : DynamicQueryParamsDto
{
    Task<ActionResult<TResponseDto>> Get([FromQuery] QueryParamsDto queryParams);

    Task<ActionResult<TEntity>> Create([FromBody] TCreateDto createDto);

    Task<ActionResult<TEntity>> Update(long id, [FromBody] TUpdateDto updateDto);

    Task<ActionResult<bool>> Delete(long id);

    Task<ActionResult<TResponseDto>> Find([FromQuery] QueryParamsDto findParams);

    Task<ActionResult<TResponseDto>> Search([FromQuery] TDynamicQueryDto searchParams);
}

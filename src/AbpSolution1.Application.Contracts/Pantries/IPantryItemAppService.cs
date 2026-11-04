using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AbpSolution1.Pantries;

/*
 * Despensa del usuario autenticado (RF-10/RF-11). Todas las operaciones actúan sólo sobre
 * los ítems propios; un ítem de otro usuario responde 404 (no se revela que existe).
 */
public interface IPantryItemAppService : IApplicationService
{
    Task<PagedResultDto<PantryItemDto>> GetListAsync(GetPantryItemsInput input);

    Task<PantryItemDto> GetAsync(Guid id);

    Task<PantryItemDto> CreateAsync(CreatePantryItemDto input);

    Task<PantryItemDto> UpdateAsync(Guid id, UpdatePantryItemDto input);

    Task<PantryItemDto> MarkAsConsumedAsync(Guid id);

    Task DeleteAsync(Guid id);
}

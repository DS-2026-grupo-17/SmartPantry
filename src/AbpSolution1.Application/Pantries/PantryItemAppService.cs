using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AbpSolution1.Products;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace AbpSolution1.Pantries;

/*
 * RF-10/RF-11: despensa del usuario autenticado.
 * - El propietario se obtiene siempre de ICurrentUser, nunca de la petición.
 * - Antes de leer, modificar, marcar o eliminar se comprueba que el ítem esté en la despensa
 *   del usuario actual. Si no, responde 404 en todas las operaciones (no se revela si existe).
 * - Las reglas de cantidad, unidad y fecha se validan en los DTO y en la entidad.
 */
[Authorize]
public class PantryItemAppService : ApplicationService, IPantryItemAppService
{
    private readonly IRepository<Pantry, Guid> _pantryRepository;
    private readonly IRepository<Product, Guid> _productRepository;

    public PantryItemAppService(
        IRepository<Pantry, Guid> pantryRepository,
        IRepository<Product, Guid> productRepository)
    {
        _pantryRepository = pantryRepository;
        _productRepository = productRepository;
    }

    public async Task<PagedResultDto<PantryItemDto>> GetListAsync(GetPantryItemsInput input)
    {
        var ownerId = CurrentUser.GetId();

        // Los ítems se consultan a través de la raíz del agregado (Pantry).
        var query =
            from pantry in await _pantryRepository.GetQueryableAsync()
            from item in pantry.Items
            join product in await _productRepository.GetQueryableAsync() on item.ProductId equals product.Id
            where pantry.OwnerId == ownerId && (input.IncludeConsumed || !item.IsConsumed)
            select new PantryItemDto
            {
                Id = item.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                ProductBarcode = product.Barcode,
                Quantity = item.Quantity,
                Unit = item.Unit,
                ExpirationDate = item.ExpirationDate,
                IsConsumed = item.IsConsumed
            };

        var totalCount = await AsyncExecuter.CountAsync(query);

        // Orden por defecto: pendientes primero, luego por fecha de vencimiento (sin fecha al final).
        var ordered = input.Sorting.IsNullOrWhiteSpace()
            ? query.OrderBy(x => x.IsConsumed)
                .ThenBy(x => x.ExpirationDate == null)
                .ThenBy(x => x.ExpirationDate)
                .ThenBy(x => x.ProductName)
            : query.OrderBy(input.Sorting!);

        var items = await AsyncExecuter.ToListAsync(ordered.Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<PantryItemDto>(totalCount, items);
    }

    public async Task<PantryItemDto> GetAsync(Guid id)
    {
        var pantry = await GetCurrentUserPantryAsync(id);
        return await MapToDtoAsync(pantry.GetItem(id));
    }

    public async Task<PantryItemDto> CreateAsync(CreatePantryItemDto input)
    {
        var ownerId = CurrentUser.GetId();
        var product = await _productRepository.GetAsync(input.ProductId);

        var pantry = await FindPantryWithItemsAsync(ownerId);
        var isNew = pantry == null;
        pantry ??= new Pantry(GuidGenerator.Create(), ownerId);

        var item = pantry.AddItem(GuidGenerator.Create(), product.Id, input.ExpirationDate, input.Quantity, input.Unit);

        if (isNew)
        {
            await _pantryRepository.InsertAsync(pantry, autoSave: true);
        }
        else
        {
            await _pantryRepository.UpdateAsync(pantry, autoSave: true);
        }

        return MapToDto(item, product);
    }

    public async Task<PantryItemDto> UpdateAsync(Guid id, UpdatePantryItemDto input)
    {
        var pantry = await GetCurrentUserPantryAsync(id);
        var item = pantry.UpdateItem(id, input.Quantity, input.Unit, input.ExpirationDate);
        await _pantryRepository.UpdateAsync(pantry, autoSave: true);
        return await MapToDtoAsync(item);
    }

    public async Task<PantryItemDto> MarkAsConsumedAsync(Guid id)
    {
        var pantry = await GetCurrentUserPantryAsync(id);
        pantry.MarkItemAsConsumed(id);
        await _pantryRepository.UpdateAsync(pantry, autoSave: true);
        return await MapToDtoAsync(pantry.GetItem(id));
    }

    public async Task DeleteAsync(Guid id)
    {
        var pantry = await GetCurrentUserPantryAsync(id);
        pantry.RemoveItem(id);
        await _pantryRepository.UpdateAsync(pantry, autoSave: true);
    }

    // Devuelve la despensa del usuario actual sólo si contiene el ítem; si no, 404.
    private async Task<Pantry> GetCurrentUserPantryAsync(Guid itemId)
    {
        var pantry = await FindPantryWithItemsAsync(CurrentUser.GetId());
        if (pantry == null || pantry.Items.All(item => item.Id != itemId))
        {
            throw new EntityNotFoundException(typeof(PantryItem), itemId);
        }

        return pantry;
    }

    private async Task<Pantry?> FindPantryWithItemsAsync(Guid ownerId)
    {
        var queryable = await _pantryRepository.WithDetailsAsync(pantry => pantry.Items);
        return await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(pantry => pantry.OwnerId == ownerId));
    }

    private async Task<PantryItemDto> MapToDtoAsync(PantryItem item)
    {
        var product = await _productRepository.GetAsync(item.ProductId);
        return MapToDto(item, product);
    }

    private static PantryItemDto MapToDto(PantryItem item, Product product)
    {
        return new PantryItemDto
        {
            Id = item.Id,
            ProductId = product.Id,
            ProductName = product.Name,
            ProductBarcode = product.Barcode,
            Quantity = item.Quantity,
            Unit = item.Unit,
            ExpirationDate = item.ExpirationDate,
            IsConsumed = item.IsConsumed
        };
    }
}

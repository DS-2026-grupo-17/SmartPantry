using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities;

namespace AbpSolution1.Pantries;

/*
 * Pantry Aggregate (wiki). Cada despensa pertenece a un único usuario.
 * OwnerId es el Id del usuario de ABP Identity (el mismo que entrega ICurrentUser);
 * se referencia sólo por Id, sin clave foránea a las tablas del módulo Identity.
 */
public class Pantry : AggregateRoot<Guid>
{
    public Guid OwnerId { get; private set; }
    public ICollection<PantryItem> Items { get; private set; }

    protected Pantry()
    {
        Items = new List<PantryItem>();
    }

    public Pantry(Guid id, Guid ownerId) : base(id)
    {
        if (ownerId == Guid.Empty)
        {
            throw new ArgumentException("La despensa debe tener un propietario.", nameof(ownerId));
        }

        OwnerId = ownerId;
        Items = new List<PantryItem>();
    }

    public PantryItem AddItem(Guid itemId, Guid productId, DateOnly? expirationDate, decimal? quantity = null, string? unit = null)
    {
        var item = new PantryItem(itemId, Id, productId, expirationDate, quantity, unit);
        Items.Add(item);
        return item;
    }

    public PantryItem UpdateItem(Guid itemId, decimal? quantity, string? unit, DateOnly? expirationDate)
    {
        var item = GetItem(itemId);
        item.Update(quantity, unit, expirationDate);
        return item;
    }

    public void ChangeItemExpirationDate(Guid itemId, DateOnly? expirationDate)
    {
        GetItem(itemId).ChangeExpirationDate(expirationDate);
    }

    public void MarkItemAsConsumed(Guid itemId)
    {
        GetItem(itemId).MarkAsConsumed();
    }

    // Eliminación explícita: sus advertencias se borran en cascada en la base.
    public void RemoveItem(Guid itemId)
    {
        Items.Remove(GetItem(itemId));
    }

    public PantryItem GetItem(Guid itemId)
    {
        return Items.FirstOrDefault(item => item.Id == itemId)
               ?? throw new EntityNotFoundException(typeof(PantryItem), itemId);
    }
}

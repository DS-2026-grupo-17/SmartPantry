using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities;

namespace AbpSolution1.Pantries;

/*
 * Pantry Aggregate (wiki). Cada despensa pertenece a un único usuario (OwnerId),
 * que se toma de datos persistidos y nunca del usuario HTTP actual.
 * Modelo mínimo para RF-15: la administración por el usuario (RF-10/RF-11) llega después.
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

    public PantryItem AddItem(Guid itemId, Guid productId, DateOnly? expirationDate)
    {
        var item = new PantryItem(itemId, Id, productId, expirationDate);
        Items.Add(item);
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

    private PantryItem GetItem(Guid itemId)
    {
        return Items.FirstOrDefault(item => item.Id == itemId)
               ?? throw new EntityNotFoundException(typeof(PantryItem), itemId);
    }
}

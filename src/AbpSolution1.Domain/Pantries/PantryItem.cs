using System;
using Volo.Abp.Domain.Entities;

namespace AbpSolution1.Pantries;

// Ítem de despensa: producto, fecha de vencimiento opcional y estado (consumido o no).
// Se modifica sólo a través de Pantry.
public class PantryItem : Entity<Guid>
{
    public Guid PantryId { get; private set; }
    public Guid ProductId { get; private set; }
    public DateOnly? ExpirationDate { get; private set; }

    // Regla de negocio 6: un ítem consumido no se elimina automáticamente, se conserva como historial.
    public bool IsConsumed { get; private set; }

    protected PantryItem() { }

    internal PantryItem(Guid id, Guid pantryId, Guid productId, DateOnly? expirationDate) : base(id)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("El ítem debe referenciar un producto.", nameof(productId));
        }

        PantryId = pantryId;
        ProductId = productId;
        ExpirationDate = expirationDate;
    }

    internal void ChangeExpirationDate(DateOnly? expirationDate)
    {
        ExpirationDate = expirationDate;
    }

    internal void MarkAsConsumed()
    {
        IsConsumed = true;
    }
}

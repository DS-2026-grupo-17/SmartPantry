using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace AbpSolution1.Pantries;

// Ítem de despensa: producto, cantidad y unidad opcionales, fecha de vencimiento opcional
// y estado (consumido o no). Se modifica sólo a través de Pantry.
public class PantryItem : Entity<Guid>
{
    public Guid PantryId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal? Quantity { get; private set; }
    public string? Unit { get; private set; }
    public DateOnly? ExpirationDate { get; private set; }

    // Regla de negocio 6: un ítem consumido no se elimina automáticamente, se conserva como historial.
    public bool IsConsumed { get; private set; }

    protected PantryItem() { }

    internal PantryItem(Guid id, Guid pantryId, Guid productId, DateOnly? expirationDate, decimal? quantity, string? unit)
        : base(id)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("El ítem debe referenciar un producto.", nameof(productId));
        }

        PantryId = pantryId;
        ProductId = productId;
        ExpirationDate = expirationDate;
        Quantity = NormalizeQuantity(quantity);
        Unit = NormalizeUnit(unit);
    }

    // Valida todo antes de asignar: una entrada inválida no deja cambios parciales.
    internal void Update(decimal? quantity, string? unit, DateOnly? expirationDate)
    {
        var normalizedQuantity = NormalizeQuantity(quantity);
        var normalizedUnit = NormalizeUnit(unit);

        Quantity = normalizedQuantity;
        Unit = normalizedUnit;
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

    private static decimal? NormalizeQuantity(decimal? quantity)
    {
        if (quantity is < 0 or > (decimal)PantryConsts.MaxQuantity)
        {
            throw new ArgumentException("La cantidad no puede ser negativa.", nameof(quantity));
        }

        return quantity;
    }

    private static string? NormalizeUnit(string? unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
        {
            return null;
        }

        return Check.Length(unit.Trim(), nameof(unit), PantryConsts.MaxUnitLength);
    }
}

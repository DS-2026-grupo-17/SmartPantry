using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace AbpSolution1.Warnings;

/*
 * Advertencia sobre un ítem de despensa. Clave lógica estable: (PantryItemId, Type),
 * reforzada con un índice único en la base. La advertencia no se borra cuando deja de
 * corresponder: se desactiva, y se reactiva si vuelve a corresponder.
 */
public class PantryWarning : AuditedAggregateRoot<Guid>
{
    public Guid OwnerId { get; private set; }
    public Guid PantryItemId { get; private set; }
    public PantryWarningType Type { get; private set; }
    public DateOnly? ExpirationDate { get; private set; }
    public bool IsActive { get; private set; }

    protected PantryWarning() { }

    public PantryWarning(Guid id, Guid ownerId, Guid pantryItemId, PantryWarningType type, DateOnly? expirationDate)
        : base(id)
    {
        OwnerId = ownerId;
        PantryItemId = pantryItemId;
        Type = type;
        ExpirationDate = expirationDate;
        IsActive = true;
    }

    // Devuelve true si cambió el estado (reactivación o nueva fecha).
    public bool ActivateForExpiration(DateOnly expirationDate)
    {
        if (IsActive && ExpirationDate == expirationDate)
        {
            return false;
        }

        IsActive = true;
        ExpirationDate = expirationDate;
        return true;
    }

    // Devuelve true si estaba activa.
    public bool Deactivate()
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        return true;
    }
}

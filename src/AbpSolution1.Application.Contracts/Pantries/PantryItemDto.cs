using System;
using Volo.Abp.Application.Dtos;

namespace AbpSolution1.Pantries;

// Los datos que no se cargaron quedan en null (RF-09): la interfaz lo comunica sin inventar valores.
public class PantryItemDto : EntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductBarcode { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public bool IsConsumed { get; set; }
}

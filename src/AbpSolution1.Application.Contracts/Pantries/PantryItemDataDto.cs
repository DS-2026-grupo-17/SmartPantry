using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AbpSolution1.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace AbpSolution1.Pantries;

// Datos editables de un ítem (RF-10/RF-11). La validación en servidor es la que manda;
// la interfaz repite estas reglas sólo por usabilidad.
public abstract class PantryItemDataDto : IValidatableObject
{
    public const int MinExpirationYear = 2000;
    public const int MaxExpirationYear = 2100;

    [Range(typeof(decimal), "0", "1000000")]
    public decimal? Quantity { get; set; }

    [StringLength(PantryConsts.MaxUnitLength)]
    public string? Unit { get; set; }

    public DateOnly? ExpirationDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ExpirationDate is { } date && (date.Year < MinExpirationYear || date.Year > MaxExpirationYear))
        {
            var localizer = validationContext.GetRequiredService<IStringLocalizer<AbpSolution1Resource>>();
            yield return new ValidationResult(localizer["Pantry:Validation:InvalidDate"], new[] { nameof(ExpirationDate) });
        }
    }
}

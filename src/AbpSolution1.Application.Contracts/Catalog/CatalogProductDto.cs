using System.Collections.Generic;

namespace AbpSolution1.Catalog;

// Los campos que el proveedor no informa quedan en null (RF-09): no se inventan valores.
public class CatalogProductDto
{
    public string Barcode { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Brand { get; set; }
    public string? ImageUrl { get; set; }
    public string? NutriScore { get; set; }
    public int? NovaGroup { get; set; }

    // null: el proveedor no informó alérgenos. Lista vacía: informó que no hay.
    public List<string>? Allergens { get; set; }
}

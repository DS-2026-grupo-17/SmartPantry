using System.Collections.Generic;

namespace AbpSolution1.Catalog;

// DTO interno entre el cliente externo y el AppService. No se expone en el endpoint.
public class ExternalProductDto
{
    public string Barcode { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Brand { get; set; }
    public string? ImageUrl { get; set; }
    public string? NutriScore { get; set; }
    public int? NovaGroup { get; set; }
    public List<string>? Allergens { get; set; }
}

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbpSolution1.OpenFoodFacts;

// Clases internas: sólo los campos de la respuesta v3 que usa SmartPantry.
// No forman parte del contrato del endpoint propio.
internal class OpenFoodFactsProductResponse
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("product")]
    public OpenFoodFactsProduct? Product { get; set; }
}

internal class OpenFoodFactsProduct
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("product_name")]
    public string? ProductName { get; set; }

    [JsonPropertyName("brands")]
    public string? Brands { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("nutriscore_grade")]
    public string? NutriscoreGrade { get; set; }

    [JsonPropertyName("nova_group")]
    public int? NovaGroup { get; set; }

    [JsonPropertyName("allergens_tags")]
    public List<string>? AllergensTags { get; set; }
}

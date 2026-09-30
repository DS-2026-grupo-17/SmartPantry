using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AbpSolution1.Catalog;

namespace AbpSolution1.OpenFoodFacts;

/*
 * Implementación de IExternalProductCatalogClient sobre Open Food Facts (Product Opener API v3).
 * El HttpClient lo entrega IHttpClientFactory ya configurado (URL base, timeout y User-Agent)
 * desde AbpSolution1HttpApiHostModule. Si el proveedor cambia su respuesta, se adapta sólo esta clase.
 */
public class OpenFoodFactsProductCatalogClient : IExternalProductCatalogClient
{
    private const string ProductPath = "api/v3/product/";
    private const string RequestedFields = "code,product_name,brands,image_url,nutriscore_grade,nova_group,allergens_tags";
    private static readonly string[] ValidNutriScores = ["a", "b", "c", "d", "e"];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private readonly HttpClient _httpClient;

    public OpenFoodFactsProductCatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExternalProductDto?> GetByBarcodeAsync(string barcode)
    {
        var requestUri = $"{ProductPath}{Uri.EscapeDataString(barcode)}?fields={RequestedFields}";

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(requestUri);
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalCatalogUnavailableException("No se pudo conectar con Open Food Facts.", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ExternalCatalogUnavailableException("Open Food Facts no respondió dentro del tiempo máximo de espera.", ex);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new ExternalCatalogRateLimitException("Se superó el límite de consultas de Open Food Facts.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalCatalogUnavailableException($"Open Food Facts respondió HTTP {(int)response.StatusCode}.");
            }

            OpenFoodFactsProductResponse? body;
            try
            {
                body = await response.Content.ReadFromJsonAsync<OpenFoodFactsProductResponse>(JsonOptions);
            }
            catch (JsonException ex)
            {
                throw new ExternalCatalogUnavailableException("Open Food Facts devolvió una respuesta con formato inesperado.", ex);
            }

            if (body?.Product == null)
            {
                return null;
            }

            return MapToExternalProductDto(body.Product, body.Code ?? barcode);
        }
    }

    private static ExternalProductDto MapToExternalProductDto(OpenFoodFactsProduct product, string barcode)
    {
        return new ExternalProductDto
        {
            Barcode = NullIfWhiteSpace(product.Code) ?? barcode,
            Name = NullIfWhiteSpace(product.ProductName),
            Brand = NullIfWhiteSpace(product.Brands),
            ImageUrl = NullIfWhiteSpace(product.ImageUrl),
            NutriScore = NormalizeNutriScore(product.NutriscoreGrade),
            NovaGroup = product.NovaGroup is >= 1 and <= 4 ? product.NovaGroup : null,
            Allergens = product.AllergensTags?
                .Select(RemoveLanguagePrefix)
                .Where(tag => tag.Length > 0)
                .ToList()
        };
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    // Open Food Facts informa "unknown" o "not-applicable" cuando no hay Nutri-Score: se deja en null.
    private static string? NormalizeNutriScore(string? grade)
    {
        var normalized = NullIfWhiteSpace(grade)?.ToLowerInvariant();
        return normalized != null && ValidNutriScores.Contains(normalized) ? normalized.ToUpperInvariant() : null;
    }

    // "en:milk" -> "milk"
    private static string RemoveLanguagePrefix(string tag)
    {
        var separatorIndex = tag.IndexOf(':');
        return (separatorIndex >= 0 ? tag[(separatorIndex + 1)..] : tag).Trim();
    }
}

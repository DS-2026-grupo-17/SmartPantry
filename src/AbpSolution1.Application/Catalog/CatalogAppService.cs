using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;

namespace AbpSolution1.Catalog;

/*
 * RF-05, búsqueda por código. Coordina la consulta al catálogo externo y traduce su resultado
 * al contrato propio. No conoce HttpClient, rutas, encabezados ni el JSON del proveedor.
 * Requiere un usuario autenticado (TP09); no exige permisos administrativos.
 * El guardado del producto (RF-08) lo resuelve ProductAppService.
 */
[Authorize]
public class CatalogAppService : ApplicationService, ICatalogAppService
{
    private readonly IExternalProductCatalogClient _externalProductCatalogClient;

    public CatalogAppService(IExternalProductCatalogClient externalProductCatalogClient)
    {
        _externalProductCatalogClient = externalProductCatalogClient;
    }

    public async Task<ProductLookupResultDto> GetByBarcodeAsync(GetProductByBarcodeDto input)
    {
        var barcode = input.Barcode.Trim();

        try
        {
            var externalProduct = await _externalProductCatalogClient.GetByBarcodeAsync(barcode);

            if (externalProduct == null)
            {
                return CreateResult(ProductLookupStatus.NotFound, barcode);
            }

            var result = CreateResult(ProductLookupStatus.Found, barcode);
            result.Product = MapToCatalogProductDto(externalProduct, barcode);
            return result;
        }
        catch (ExternalCatalogRateLimitException)
        {
            return CreateResult(ProductLookupStatus.RateLimited, barcode);
        }
        catch (ExternalCatalogUnavailableException)
        {
            return CreateResult(ProductLookupStatus.Unavailable, barcode);
        }
    }

    private static ProductLookupResultDto CreateResult(ProductLookupStatus status, string barcode)
    {
        return new ProductLookupResultDto
        {
            Status = status,
            Barcode = barcode
        };
    }

    private static CatalogProductDto MapToCatalogProductDto(ExternalProductDto externalProduct, string requestedBarcode)
    {
        return new CatalogProductDto
        {
            Barcode = string.IsNullOrWhiteSpace(externalProduct.Barcode) ? requestedBarcode : externalProduct.Barcode,
            Name = externalProduct.Name,
            Brand = externalProduct.Brand,
            ImageUrl = externalProduct.ImageUrl,
            NutriScore = externalProduct.NutriScore,
            NovaGroup = externalProduct.NovaGroup,
            Allergens = externalProduct.Allergens?.ToList()
        };
    }
}

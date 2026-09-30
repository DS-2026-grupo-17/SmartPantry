using System.Threading.Tasks;

namespace AbpSolution1.Catalog;

/*
 * Necesidad de la aplicación: consultar un catálogo externo de productos.
 * No depende de HttpClient ni de Open Food Facts.
 * - Devuelve null cuando el producto no existe.
 * - Lanza ExternalCatalogRateLimitException si se superó el límite de consultas.
 * - Lanza ExternalCatalogUnavailableException si el proveedor no está disponible.
 */
public interface IExternalProductCatalogClient
{
    Task<ExternalProductDto?> GetByBarcodeAsync(string barcode);
}

namespace AbpSolution1.Catalog;

public class ProductLookupResultDto
{
    public ProductLookupStatus Status { get; set; }
    public string Barcode { get; set; } = string.Empty;

    // Sólo tiene valor cuando Status es Found.
    public CatalogProductDto? Product { get; set; }
}

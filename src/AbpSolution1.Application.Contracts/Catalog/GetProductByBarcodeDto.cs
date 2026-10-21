using System.ComponentModel.DataAnnotations;
using AbpSolution1.Products;

namespace AbpSolution1.Catalog;

public class GetProductByBarcodeDto
{
    public const string BarcodePattern = ProductConsts.BarcodePattern;

    [Required]
    [RegularExpression(BarcodePattern, ErrorMessage = "El código de barras debe tener entre 8 y 14 dígitos numéricos.")]
    public string Barcode { get; set; } = string.Empty;
}

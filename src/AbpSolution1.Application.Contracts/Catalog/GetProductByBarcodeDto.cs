using System.ComponentModel.DataAnnotations;

namespace AbpSolution1.Catalog;

public class GetProductByBarcodeDto
{
    // EAN-8, UPC-A (12), EAN-13 y GTIN-14: sólo dígitos, entre 8 y 14.
    public const string BarcodePattern = @"^\d{8,14}$";

    [Required]
    [RegularExpression(BarcodePattern, ErrorMessage = "El código de barras debe tener entre 8 y 14 dígitos numéricos.")]
    public string Barcode { get; set; } = string.Empty;
}

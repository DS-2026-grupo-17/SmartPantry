using System.ComponentModel.DataAnnotations;

namespace AbpSolution1.Products;

public class SaveProductDto
{
    [Required]
    [StringLength(ProductConsts.MaxBarcodeLength)]
    [RegularExpression(ProductConsts.BarcodePattern, ErrorMessage = "El código de barras debe tener entre 8 y 14 dígitos numéricos.")]
    public string Barcode { get; set; } = string.Empty;

    [Required]
    [StringLength(ProductConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;
}

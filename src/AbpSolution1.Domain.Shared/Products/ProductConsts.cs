namespace AbpSolution1.Products;

public static class ProductConsts
{
    public const int MaxBarcodeLength = 14;
    public const int MaxNameLength = 256;

    // EAN-8, UPC-A (12), EAN-13 y GTIN-14: sólo dígitos, entre 8 y 14.
    public const string BarcodePattern = @"^\d{8,14}$";
}

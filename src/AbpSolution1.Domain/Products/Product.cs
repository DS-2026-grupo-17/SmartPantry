using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace AbpSolution1.Products;

/*
 * Modelo mínimo del Product (wiki: Product Aggregate) para que un ítem de despensa
 * referencie un producto. Guardar productos de Open Food Facts (RF-08) es un incremento posterior.
 */
public class Product : AggregateRoot<Guid>
{
    public string Barcode { get; private set; }
    public string Name { get; private set; }

    protected Product() { }

    public Product(Guid id, string barcode, string name) : base(id)
    {
        Barcode = Check.NotNullOrWhiteSpace(barcode, nameof(barcode), ProductConsts.MaxBarcodeLength).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ProductConsts.MaxNameLength).Trim();
    }
}

using System;
using Volo.Abp.Application.Dtos;

namespace AbpSolution1.Products;

public class ProductDto : EntityDto<Guid>
{
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

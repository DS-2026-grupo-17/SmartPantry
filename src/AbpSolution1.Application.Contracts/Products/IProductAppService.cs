using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AbpSolution1.Products;

public interface IProductAppService : IApplicationService
{
    Task<ProductDto> GetAsync(Guid id);

    Task<PagedResultDto<ProductDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    // RF-08: guarda un producto en el catálogo interno sin duplicar el código de barras.
    Task<ProductDto> SaveAsync(SaveProductDto input);
}

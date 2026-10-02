using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace AbpSolution1.Catalog;

public interface ICatalogAppService : IApplicationService
{
    Task<ProductLookupResultDto> GetByBarcodeAsync(GetProductByBarcodeDto input);
}

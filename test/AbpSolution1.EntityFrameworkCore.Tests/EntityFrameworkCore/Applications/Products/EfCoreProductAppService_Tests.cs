using AbpSolution1.Products;
using Xunit;

namespace AbpSolution1.EntityFrameworkCore.Applications.Products;

[Collection(AbpSolution1TestConsts.CollectionDefinitionName)]
public class EfCoreProductAppService_Tests : ProductAppService_Tests<AbpSolution1EntityFrameworkCoreTestModule>
{
}

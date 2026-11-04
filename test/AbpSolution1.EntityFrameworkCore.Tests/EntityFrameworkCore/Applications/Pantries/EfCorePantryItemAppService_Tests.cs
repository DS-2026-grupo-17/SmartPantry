using AbpSolution1.Pantries;
using Xunit;

namespace AbpSolution1.EntityFrameworkCore.Applications.Pantries;

[Collection(AbpSolution1TestConsts.CollectionDefinitionName)]
public class EfCorePantryItemAppService_Tests : PantryItemAppService_Tests<AbpSolution1EntityFrameworkCoreTestModule>
{
}

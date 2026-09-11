using AbpSolution1.Books;
using Xunit;

namespace AbpSolution1.EntityFrameworkCore.Applications.Books;

[Collection(AbpSolution1TestConsts.CollectionDefinitionName)]
public class EfCoreBookAppService_Tests : BookAppService_Tests<AbpSolution1EntityFrameworkCoreTestModule>
{

}
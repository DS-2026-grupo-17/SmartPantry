using AbpSolution1.Users;
using Xunit;

namespace AbpSolution1.EntityFrameworkCore.Applications.Users;

[Collection(AbpSolution1TestConsts.CollectionDefinitionName)]
public class EfCoreUserAppService_Tests : UserAppService_Tests<AbpSolution1EntityFrameworkCoreTestModule>
{
}
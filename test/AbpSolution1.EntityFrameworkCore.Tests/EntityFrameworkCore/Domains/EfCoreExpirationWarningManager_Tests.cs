using AbpSolution1.Warnings;
using Xunit;

namespace AbpSolution1.EntityFrameworkCore.Domains;

[Collection(AbpSolution1TestConsts.CollectionDefinitionName)]
public class EfCoreExpirationWarningManager_Tests : ExpirationWarningManager_Tests<AbpSolution1EntityFrameworkCoreTestModule>
{
}

using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;

namespace AbpSolution1;

[DependsOn(
    typeof(AbpSolution1ApplicationModule),
    typeof(AbpSolution1DomainTestModule)
)]
public class AbpSolution1ApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // La suite no espera al worker periódico: la regla se prueba llamando directamente a ExpirationWarningManager.
        Configure<AbpBackgroundWorkerOptions>(options =>
        {
            options.IsEnabled = false;
        });
    }
}

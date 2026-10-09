using System;
using System.Threading.Tasks;
using AbpSolution1.Pantries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace AbpSolution1.Warnings;

/*
 * RF-15: proceso periódico, sin petición HTTP. El worker sólo decide cuándo ejecutar;
 * ExpirationWarningManager decide qué advertencias crear, actualizar o desactivar.
 * - No guarda repositorios: resuelve el servicio desde el alcance (scope) de cada ejecución.
 * - No usa ICurrentUser: el propietario sale de los datos persistidos de cada despensa.
 * - Cada ejecución es una unidad de trabajo transaccional.
 * - Los errores se registran y no detienen las ejecuciones futuras.
 */
public class ExpirationWarningWorker : AsyncPeriodicBackgroundWorkerBase
{
    public ExpirationWarningWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = PantryConsts.ExpirationWarningWorkerPeriodMilliseconds;
        Timer.RunOnStart = true;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        // Criterio de fecha: día calendario UTC (el mismo que usan las pruebas).
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        try
        {
            var unitOfWorkManager = workerContext.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            var expirationWarningManager = workerContext.ServiceProvider.GetRequiredService<ExpirationWarningManager>();

            using var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
            var result = await expirationWarningManager.ProcessAsync(today);
            await uow.CompleteAsync();

            Logger.LogInformation(
                "Vencimientos procesados para {Today}: {Created} creadas, {Updated} actualizadas, {Deactivated} desactivadas.",
                today, result.Created, result.Updated, result.Deactivated);
        }
        catch (Exception ex)
        {
            // Si otra ejecución cercana ya creó la misma advertencia, el índice único rechaza
            // el duplicado y esta transacción se revierte completa; el próximo ciclo reprocesa.
            Logger.LogError(ex, "Error al procesar vencimientos para {Today}. Se reintenta en la próxima ejecución.", today);
        }
    }
}

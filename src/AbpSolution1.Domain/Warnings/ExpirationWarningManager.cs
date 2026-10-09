using System;
using System.Linq;
using System.Threading.Tasks;
using AbpSolution1.Pantries;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace AbpSolution1.Warnings;

/*
 * RF-15: decide qué advertencias de vencimiento crear, actualizar o desactivar.
 * Recibe "hoy" (fecha UTC) por parámetro para poder probarse sin esperar al worker.
 * Es idempotente: con los mismos datos y la misma fecha, una segunda ejecución no cambia nada.
 * No abre su propia unidad de trabajo: la delimita quien lo llama (el worker o las pruebas).
 */
public class ExpirationWarningManager : DomainService
{
    private readonly IRepository<Pantry, Guid> _pantryRepository;
    private readonly IRepository<PantryWarning, Guid> _warningRepository;

    public ExpirationWarningManager(
        IRepository<Pantry, Guid> pantryRepository,
        IRepository<PantryWarning, Guid> warningRepository)
    {
        _pantryRepository = pantryRepository;
        _warningRepository = warningRepository;
    }

    public static bool RequiresWarning(PantryItem item, DateOnly today)
    {
        return !item.IsConsumed
               && item.ExpirationDate.HasValue
               && item.ExpirationDate.Value <= today.AddDays(PantryConsts.ExpirationWarningThresholdDays);
    }

    public async Task<ExpirationWarningProcessResult> ProcessAsync(DateOnly today)
    {
        var result = new ExpirationWarningProcessResult();

        var pantries = await AsyncExecuter.ToListAsync(await _pantryRepository.WithDetailsAsync(pantry => pantry.Items));
        var warnings = (await _warningRepository.GetListAsync(warning => warning.Type == PantryWarningType.Expiration))
            .ToDictionary(warning => warning.PantryItemId);

        foreach (var pantry in pantries)
        {
            foreach (var item in pantry.Items)
            {
                warnings.TryGetValue(item.Id, out var warning);

                if (RequiresWarning(item, today))
                {
                    var expirationDate = item.ExpirationDate!.Value;

                    if (warning == null)
                    {
                        await _warningRepository.InsertAsync(new PantryWarning(
                            GuidGenerator.Create(),
                            pantry.OwnerId,
                            item.Id,
                            PantryWarningType.Expiration,
                            expirationDate));
                        result.Created++;
                    }
                    else if (warning.ActivateForExpiration(expirationDate))
                    {
                        await _warningRepository.UpdateAsync(warning);
                        result.Updated++;
                    }
                }
                else if (warning != null && warning.Deactivate())
                {
                    await _warningRepository.UpdateAsync(warning);
                    result.Deactivated++;
                }
            }
        }

        return result;
    }
}

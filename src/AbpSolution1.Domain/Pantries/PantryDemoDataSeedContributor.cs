using System;
using System.Threading.Tasks;
using AbpSolution1.Products;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;

namespace AbpSolution1.Pantries;

/*
 * Datos locales de ejemplo para el worker de vencimientos (TP08) y la pantalla "Mi despensa" (TP10).
 * Se ejecuta con el DbMigrator: crea la despensa del usuario admin de ABP si todavía no tiene una.
 * Algunos ítems no tienen cantidad, unidad o fecha, para observar RF-09 en la interfaz.
 * Las fechas son relativas al día UTC de la carga.
 */
public class PantryDemoDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<Pantry, Guid> _pantryRepository;
    private readonly IGuidGenerator _guidGenerator;

    public PantryDemoDataSeedContributor(
        IIdentityUserRepository identityUserRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<Pantry, Guid> pantryRepository,
        IGuidGenerator guidGenerator)
    {
        _identityUserRepository = identityUserRepository;
        _productRepository = productRepository;
        _pantryRepository = pantryRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var admin = await _identityUserRepository.FindByNormalizedUserNameAsync("ADMIN");
        if (admin == null || await _pantryRepository.AnyAsync(pantry => pantry.OwnerId == admin.Id))
        {
            return;
        }

        var nutella = await GetOrCreateProductAsync("3017620422003", "Nutella");
        var milk = await GetOrCreateProductAsync("7790742363008", "Leche entera");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var pantry = new Pantry(_guidGenerator.Create(), admin.Id);

        pantry.AddItem(_guidGenerator.Create(), milk.Id, today.AddDays(1), 2, "l");                // dentro del umbral
        pantry.AddItem(_guidGenerator.Create(), nutella.Id, today.AddDays(PantryConsts.ExpirationWarningThresholdDays), 1, "frasco"); // día límite
        pantry.AddItem(_guidGenerator.Create(), nutella.Id, today.AddDays(30));                    // fuera del umbral, sin cantidad
        pantry.AddItem(_guidGenerator.Create(), milk.Id, today.AddDays(-2), 1, "l");               // vencido
        pantry.AddItem(_guidGenerator.Create(), nutella.Id, null);                                 // sin fecha ni cantidad

        var consumed = pantry.AddItem(_guidGenerator.Create(), milk.Id, today.AddDays(1), 1, "l"); // consumido
        pantry.MarkItemAsConsumed(consumed.Id);

        await _pantryRepository.InsertAsync(pantry, autoSave: true);
    }

    private async Task<Product> GetOrCreateProductAsync(string barcode, string name)
    {
        return await _productRepository.FindAsync(product => product.Barcode == barcode)
               ?? await _productRepository.InsertAsync(new Product(_guidGenerator.Create(), barcode, name), autoSave: true);
    }
}

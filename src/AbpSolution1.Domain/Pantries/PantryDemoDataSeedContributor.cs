using System;
using System.Threading.Tasks;
using AbpSolution1.Products;
using AbpSolution1.Users;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace AbpSolution1.Pantries;

/*
 * Datos locales de ejemplo para observar el worker de vencimientos (TP08).
 * Se ejecuta con el DbMigrator y sólo si todavía no hay ninguna despensa.
 * No existe una API pública para elegir el propietario de un ítem: eso espera a Seguridad.
 * Las fechas son relativas al día UTC de la carga.
 */
public class PantryDemoDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<User, Guid> _userRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<Pantry, Guid> _pantryRepository;
    private readonly IGuidGenerator _guidGenerator;

    public PantryDemoDataSeedContributor(
        IRepository<User, Guid> userRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<Pantry, Guid> pantryRepository,
        IGuidGenerator guidGenerator)
    {
        _userRepository = userRepository;
        _productRepository = productRepository;
        _pantryRepository = pantryRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _pantryRepository.GetCountAsync() > 0)
        {
            return;
        }

        var owner = await _userRepository.FindAsync(user => user.Email == "demo@smartpantry.local")
                    ?? await _userRepository.InsertAsync(
                        new User(_guidGenerator.Create(), "Usuario Demo", "demo@smartpantry.local", "Member"),
                        autoSave: true);

        var nutella = await GetOrCreateProductAsync("3017620422003", "Nutella");
        var milk = await GetOrCreateProductAsync("7790742363008", "Leche entera");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var pantry = new Pantry(_guidGenerator.Create(), owner.Id);

        pantry.AddItem(_guidGenerator.Create(), milk.Id, today.AddDays(1));                       // dentro del umbral
        pantry.AddItem(_guidGenerator.Create(), nutella.Id, today.AddDays(PantryConsts.ExpirationWarningThresholdDays)); // día límite
        pantry.AddItem(_guidGenerator.Create(), nutella.Id, today.AddDays(30));                   // fuera del umbral
        pantry.AddItem(_guidGenerator.Create(), milk.Id, today.AddDays(-2));                      // vencido
        pantry.AddItem(_guidGenerator.Create(), nutella.Id, null);                                // sin fecha

        var consumed = pantry.AddItem(_guidGenerator.Create(), milk.Id, today.AddDays(1));         // consumido
        pantry.MarkItemAsConsumed(consumed.Id);

        await _pantryRepository.InsertAsync(pantry, autoSave: true);
    }

    private async Task<Product> GetOrCreateProductAsync(string barcode, string name)
    {
        return await _productRepository.FindAsync(product => product.Barcode == barcode)
               ?? await _productRepository.InsertAsync(new Product(_guidGenerator.Create(), barcode, name), autoSave: true);
    }
}

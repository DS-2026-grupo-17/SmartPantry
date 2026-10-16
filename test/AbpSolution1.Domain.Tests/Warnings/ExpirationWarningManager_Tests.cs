using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbpSolution1.Pantries;
using AbpSolution1.Products;
using AbpSolution1.Users;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Xunit;

namespace AbpSolution1.Warnings;

/*
 * Prueba la regla de RF-15 llamando directamente a ExpirationWarningManager con una fecha
 * controlada (criterio UTC, igual que el worker), sin esperar al temporizador.
 * Se ejecuta contra la base SQLite aislada de la suite ABP (ver EfCoreExpirationWarningManager_Tests).
 */
public abstract class ExpirationWarningManager_Tests<TStartupModule> : AbpSolution1DomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private static readonly DateOnly Today = new(2026, 10, 9);

    private readonly ExpirationWarningManager _expirationWarningManager;
    private readonly IRepository<User, Guid> _userRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<Pantry, Guid> _pantryRepository;
    private readonly IRepository<PantryWarning, Guid> _warningRepository;

    protected ExpirationWarningManager_Tests()
    {
        _expirationWarningManager = GetRequiredService<ExpirationWarningManager>();
        _userRepository = GetRequiredService<IRepository<User, Guid>>();
        _productRepository = GetRequiredService<IRepository<Product, Guid>>();
        _pantryRepository = GetRequiredService<IRepository<Pantry, Guid>>();
        _warningRepository = GetRequiredService<IRepository<PantryWarning, Guid>>();
    }

    [Fact]
    public async Task Should_Apply_Threshold_Rules()
    {
        //Arrange
        var withinThreshold = Guid.NewGuid();
        var limitDay = Guid.NewGuid();
        var outsideThreshold = Guid.NewGuid();
        var expired = Guid.NewGuid();
        var withoutDate = Guid.NewGuid();
        var consumed = Guid.NewGuid();

        await CreatePantryAsync(
            (withinThreshold, Today.AddDays(1), false),
            (limitDay, Today.AddDays(PantryConsts.ExpirationWarningThresholdDays), false),
            (outsideThreshold, Today.AddDays(PantryConsts.ExpirationWarningThresholdDays + 1), false),
            (expired, Today.AddDays(-2), false),
            (withoutDate, null, false),
            (consumed, Today.AddDays(1), true));

        //Act
        await ProcessAsync(Today);

        //Assert
        (await FindWarningAsync(withinThreshold)).ShouldNotBeNull().IsActive.ShouldBeTrue();
        (await FindWarningAsync(limitDay)).ShouldNotBeNull().IsActive.ShouldBeTrue();
        (await FindWarningAsync(expired)).ShouldNotBeNull().IsActive.ShouldBeTrue();
        (await FindWarningAsync(outsideThreshold)).ShouldBeNull();
        (await FindWarningAsync(withoutDate)).ShouldBeNull();
        (await FindWarningAsync(consumed)).ShouldBeNull();
    }

    [Fact]
    public async Task Should_Keep_A_Single_Warning_Per_Item_When_Processed_Twice()
    {
        //Arrange
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        await CreatePantryAsync((first, Today, false), (second, Today.AddDays(2), false));

        //Act
        var firstRun = await ProcessAsync(Today);
        var secondRun = await ProcessAsync(Today);

        //Assert
        firstRun.Created.ShouldBeGreaterThanOrEqualTo(2);
        secondRun.Created.ShouldBe(0);
        secondRun.Updated.ShouldBe(0);
        secondRun.Deactivated.ShouldBe(0);

        var warnings = await _warningRepository.GetListAsync();
        warnings.Count(w => w.PantryItemId == first).ShouldBe(1);
        warnings.Count(w => w.PantryItemId == second).ShouldBe(1);
        warnings.GroupBy(w => new { w.PantryItemId, w.Type }).ShouldAllBe(group => group.Count() == 1);
    }

    [Fact]
    public async Task Should_Update_Deactivate_And_Reactivate_The_Same_Warning()
    {
        //Arrange
        var itemId = Guid.NewGuid();
        var pantryId = await CreatePantryAsync((itemId, Today.AddDays(2), false));

        await ProcessAsync(Today);
        var original = (await FindWarningAsync(itemId)).ShouldNotBeNull();
        original.ExpirationDate.ShouldBe(Today.AddDays(2));

        //Act + Assert - la fecha cambia dentro del umbral: se actualiza la misma advertencia
        await ChangeExpirationDateAsync(pantryId, itemId, Today.AddDays(1));
        var updateRun = await ProcessAsync(Today);
        var updated = (await FindWarningAsync(itemId)).ShouldNotBeNull();
        updateRun.Updated.ShouldBeGreaterThanOrEqualTo(1);
        updated.Id.ShouldBe(original.Id);
        updated.IsActive.ShouldBeTrue();
        updated.ExpirationDate.ShouldBe(Today.AddDays(1));

        //Act + Assert - la fecha sale del umbral: se desactiva
        await ChangeExpirationDateAsync(pantryId, itemId, Today.AddDays(10));
        await ProcessAsync(Today);
        var deactivated = (await FindWarningAsync(itemId)).ShouldNotBeNull();
        deactivated.Id.ShouldBe(original.Id);
        deactivated.IsActive.ShouldBeFalse();

        //Act + Assert - vuelve a entrar en el umbral: se reactiva la misma advertencia
        await ChangeExpirationDateAsync(pantryId, itemId, Today.AddDays(PantryConsts.ExpirationWarningThresholdDays));
        await ProcessAsync(Today);
        var reactivated = (await FindWarningAsync(itemId)).ShouldNotBeNull();
        reactivated.Id.ShouldBe(original.Id);
        reactivated.IsActive.ShouldBeTrue();
        reactivated.ExpirationDate.ShouldBe(Today.AddDays(PantryConsts.ExpirationWarningThresholdDays));

        (await _warningRepository.GetListAsync(w => w.PantryItemId == itemId)).Count.ShouldBe(1);
    }

    [Fact]
    public async Task Should_Deactivate_Warning_When_Item_Is_Consumed()
    {
        //Arrange
        var itemId = Guid.NewGuid();
        var pantryId = await CreatePantryAsync((itemId, Today.AddDays(1), false));
        await ProcessAsync(Today);

        //Act
        await WithUnitOfWorkAsync(async () =>
        {
            var pantry = await GetPantryWithItemsAsync(pantryId);
            pantry.MarkItemAsConsumed(itemId);
            await _pantryRepository.UpdateAsync(pantry);
        });
        await ProcessAsync(Today);

        //Assert
        (await FindWarningAsync(itemId)).ShouldNotBeNull().IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Reject_Duplicate_Warning_For_Same_Item_And_Type_In_Database()
    {
        //Arrange - simula dos ejecuciones cercanas que intentan crear la misma advertencia
        var itemId = Guid.NewGuid();
        await CreatePantryAsync((itemId, Today.AddDays(1), false));
        var ownerId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await _warningRepository.InsertAsync(
                new PantryWarning(Guid.NewGuid(), ownerId, itemId, PantryWarningType.Expiration, Today.AddDays(1)));
        });

        //Act + Assert - el índice único rechaza la segunda
        await Should.ThrowAsync<Exception>(async () =>
        {
            await WithUnitOfWorkAsync(async () =>
            {
                await _warningRepository.InsertAsync(
                    new PantryWarning(Guid.NewGuid(), ownerId, itemId, PantryWarningType.Expiration, Today.AddDays(1)));
            });
        });

        (await _warningRepository.GetListAsync(w => w.PantryItemId == itemId)).Count.ShouldBe(1);
    }

    private Task<ExpirationWarningProcessResult> ProcessAsync(DateOnly today)
    {
        return WithUnitOfWorkAsync(() => _expirationWarningManager.ProcessAsync(today));
    }

    private Task<PantryWarning?> FindWarningAsync(Guid itemId)
    {
        return _warningRepository.FindAsync(w => w.PantryItemId == itemId && w.Type == PantryWarningType.Expiration);
    }

    private Task ChangeExpirationDateAsync(Guid pantryId, Guid itemId, DateOnly? expirationDate)
    {
        return WithUnitOfWorkAsync(async () =>
        {
            var pantry = await GetPantryWithItemsAsync(pantryId);
            pantry.ChangeItemExpirationDate(itemId, expirationDate);
            await _pantryRepository.UpdateAsync(pantry);
        });
    }

    private async Task<Pantry> GetPantryWithItemsAsync(Guid pantryId)
    {
        var queryable = await _pantryRepository.WithDetailsAsync(pantry => pantry.Items);
        return queryable.Single(pantry => pantry.Id == pantryId);
    }

    private async Task<Guid> CreatePantryAsync(params (Guid ItemId, DateOnly? ExpirationDate, bool Consumed)[] items)
    {
        var pantryId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var owner = await _userRepository.InsertAsync(
                new User(Guid.NewGuid(), "Usuario de prueba", $"{Guid.NewGuid():N}@test.local", "Member"), autoSave: true);
            var product = await _productRepository.InsertAsync(
                new Product(Guid.NewGuid(), Random.Shared.NextInt64(1_000_000_000_000, 9_999_999_999_999).ToString(), "Producto de prueba"), autoSave: true);

            var pantry = new Pantry(pantryId, owner.Id);
            foreach (var item in items)
            {
                pantry.AddItem(item.ItemId, product.Id, item.ExpirationDate);
                if (item.Consumed)
                {
                    pantry.MarkItemAsConsumed(item.ItemId);
                }
            }

            await _pantryRepository.InsertAsync(pantry, autoSave: true);
        });

        return pantryId;
    }
}

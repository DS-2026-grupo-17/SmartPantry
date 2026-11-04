using System;
using System.Linq;
using System.Threading.Tasks;
using AbpSolution1.Products;
using Shouldly;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace AbpSolution1.Pantries;

/*
 * La suite usa un usuario fijo (FakeCurrentPrincipalAccessor) y AlwaysAllowAuthorization:
 * estas pruebas cubren la regla de propiedad por ICurrentUser, no el acceso HTTP (401).
 */
public abstract class PantryItemAppService_Tests<TStartupModule> : AbpSolution1ApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private static readonly DateOnly Today = new(2026, 10, 23);

    private readonly IPantryItemAppService _pantryItemAppService;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<Pantry, Guid> _pantryRepository;

    protected PantryItemAppService_Tests()
    {
        _pantryItemAppService = GetRequiredService<IPantryItemAppService>();
        _productRepository = GetRequiredService<IRepository<Product, Guid>>();
        _pantryRepository = GetRequiredService<IRepository<Pantry, Guid>>();
    }

    [Fact]
    public async Task Should_Create_List_Update_Consume_And_Delete_Own_Items()
    {
        //Arrange
        var product = await CreateProductAsync("Yerba mate");

        //Act + Assert - agregar (RF-10)
        var withData = await _pantryItemAppService.CreateAsync(new CreatePantryItemDto
        {
            ProductId = product.Id, Quantity = 2, Unit = " kg ", ExpirationDate = Today.AddDays(5)
        });
        var withoutData = await _pantryItemAppService.CreateAsync(new CreatePantryItemDto { ProductId = product.Id });

        withData.Unit.ShouldBe("kg");

        //Listar con el nombre del producto
        var list = await _pantryItemAppService.GetListAsync(new GetPantryItemsInput());
        list.TotalCount.ShouldBe(2);
        list.Items.ShouldAllBe(item => item.ProductName == "Yerba mate");

        //Datos faltantes quedan en null (RF-09)
        var missing = list.Items.Single(item => item.Id == withoutData.Id);
        missing.Quantity.ShouldBeNull();
        missing.Unit.ShouldBeNull();
        missing.ExpirationDate.ShouldBeNull();

        //Editar
        var updated = await _pantryItemAppService.UpdateAsync(withData.Id, new UpdatePantryItemDto
        {
            Quantity = 1.5m, Unit = "kg", ExpirationDate = Today.AddDays(10)
        });
        updated.Quantity.ShouldBe(1.5m);
        updated.ExpirationDate.ShouldBe(Today.AddDays(10));

        //Marcar como consumido: se conserva como historial
        var consumed = await _pantryItemAppService.MarkAsConsumedAsync(withData.Id);
        consumed.IsConsumed.ShouldBeTrue();
        (await _pantryItemAppService.GetListAsync(new GetPantryItemsInput { IncludeConsumed = true })).TotalCount.ShouldBe(2);
        (await _pantryItemAppService.GetListAsync(new GetPantryItemsInput { IncludeConsumed = false })).TotalCount.ShouldBe(1);

        //Eliminar
        await _pantryItemAppService.DeleteAsync(withData.Id);
        await Assert.ThrowsAnyAsync<EntityNotFoundException>(() => _pantryItemAppService.GetAsync(withData.Id));
        (await _pantryItemAppService.GetListAsync(new GetPantryItemsInput())).TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task Should_Not_Read_Or_Modify_Items_Of_Another_User()
    {
        //Arrange - ítem en la despensa de otro usuario
        var product = await CreateProductAsync("Leche");
        var otherItemId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
        {
            var otherPantry = new Pantry(Guid.NewGuid(), Guid.NewGuid());
            otherPantry.AddItem(otherItemId, product.Id, Today.AddDays(1), 1, "l");
            await _pantryRepository.InsertAsync(otherPantry, autoSave: true);
        });

        //Act + Assert - no aparece en el listado y todas las operaciones responden 404
        (await _pantryItemAppService.GetListAsync(new GetPantryItemsInput())).Items.ShouldNotContain(item => item.Id == otherItemId);
        await Assert.ThrowsAnyAsync<EntityNotFoundException>(() => _pantryItemAppService.GetAsync(otherItemId));
        await Assert.ThrowsAnyAsync<EntityNotFoundException>(() =>
            _pantryItemAppService.UpdateAsync(otherItemId, new UpdatePantryItemDto { Quantity = 99 }));
        await Assert.ThrowsAnyAsync<EntityNotFoundException>(() => _pantryItemAppService.MarkAsConsumedAsync(otherItemId));
        await Assert.ThrowsAnyAsync<EntityNotFoundException>(() => _pantryItemAppService.DeleteAsync(otherItemId));

        //El ítem ajeno quedó intacto
        await WithUnitOfWorkAsync(async () =>
        {
            var otherPantry = (await _pantryRepository.WithDetailsAsync(p => p.Items))
                .Single(p => p.Items.Any(item => item.Id == otherItemId));
            var item = otherPantry.GetItem(otherItemId);
            item.Quantity.ShouldBe(1);
            item.IsConsumed.ShouldBeFalse();
        });
    }

    [Fact]
    public async Task Should_Reject_Invalid_Update_And_Keep_Previous_Data()
    {
        //Arrange
        var product = await CreateProductAsync("Arroz");
        var created = await _pantryItemAppService.CreateAsync(new CreatePantryItemDto
        {
            ProductId = product.Id, Quantity = 3, Unit = "paquete", ExpirationDate = Today.AddDays(30)
        });

        //Act + Assert - cantidad negativa
        var negative = await Assert.ThrowsAsync<AbpValidationException>(() =>
            _pantryItemAppService.UpdateAsync(created.Id, new UpdatePantryItemDto { Quantity = -1 }));
        negative.ValidationErrors.ShouldContain(err => err.MemberNames.Contains(nameof(UpdatePantryItemDto.Quantity)));

        //Fecha fuera de rango
        var invalidDate = await Assert.ThrowsAsync<AbpValidationException>(() =>
            _pantryItemAppService.UpdateAsync(created.Id, new UpdatePantryItemDto { ExpirationDate = new DateOnly(1900, 1, 1) }));
        invalidDate.ValidationErrors.ShouldContain(err => err.MemberNames.Contains(nameof(UpdatePantryItemDto.ExpirationDate)));

        //Unidad demasiado larga
        await Assert.ThrowsAsync<AbpValidationException>(() =>
            _pantryItemAppService.UpdateAsync(created.Id, new UpdatePantryItemDto
            {
                Unit = new string('u', PantryConsts.MaxUnitLength + 1)
            }));

        //Los datos guardados no cambiaron
        var fetched = await _pantryItemAppService.GetAsync(created.Id);
        fetched.Quantity.ShouldBe(3);
        fetched.Unit.ShouldBe("paquete");
        fetched.ExpirationDate.ShouldBe(Today.AddDays(30));
    }

    [Fact]
    public async Task Should_Return_Not_Found_When_Adding_A_Product_That_Does_Not_Exist()
    {
        await Assert.ThrowsAnyAsync<EntityNotFoundException>(() =>
            _pantryItemAppService.CreateAsync(new CreatePantryItemDto { ProductId = Guid.NewGuid() }));
    }

    private async Task<Product> CreateProductAsync(string name)
    {
        var barcode = Random.Shared.NextInt64(1_000_000_000_000, 9_999_999_999_999).ToString();
        return await _productRepository.InsertAsync(new Product(Guid.NewGuid(), barcode, name), autoSave: true);
    }
}

using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace AbpSolution1.Products;

// La suite usa AlwaysAllowAuthorization: estas pruebas cubren la regla de RF-08, no el acceso HTTP.
public abstract class ProductAppService_Tests<TStartupModule> : AbpSolution1ApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IProductAppService _productAppService;

    protected ProductAppService_Tests()
    {
        _productAppService = GetRequiredService<IProductAppService>();
    }

    [Fact]
    public async Task Should_Save_And_Get_A_Product()
    {
        //Act
        var saved = await _productAppService.SaveAsync(new SaveProductDto { Barcode = "7790580100211", Name = "  Yerba mate  " });
        var fetched = await _productAppService.GetAsync(saved.Id);

        //Assert
        fetched.Barcode.ShouldBe("7790580100211");
        fetched.Name.ShouldBe("Yerba mate");
    }

    [Fact]
    public async Task Should_Not_Duplicate_A_Product_With_The_Same_Barcode()
    {
        //Act
        var first = await _productAppService.SaveAsync(new SaveProductDto { Barcode = "7790070012345", Name = "Galletitas" });
        var second = await _productAppService.SaveAsync(new SaveProductDto { Barcode = "7790070012345", Name = "Galletitas" });

        //Assert
        second.Id.ShouldBe(first.Id);
        var list = await _productAppService.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 1000 });
        list.Items.Count(p => p.Barcode == "7790070012345").ShouldBe(1);
    }

    [Fact]
    public async Task Should_Not_Save_A_Product_With_Invalid_Barcode()
    {
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _productAppService.SaveAsync(new SaveProductDto { Barcode = "12AB", Name = "Producto" });
        });

        exception.ValidationErrors.ShouldContain(err => err.MemberNames.Any(mem => mem == "Barcode"));
    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;

namespace AbpSolution1.Catalog;

// Pruebas unitarias: IExternalProductCatalogClient se reemplaza por un mock, sin Internet.
public class CatalogAppService_Tests
{
    private const string Barcode = "3017620422003";

    private readonly IExternalProductCatalogClient _externalProductCatalogClient;
    private readonly CatalogAppService _catalogAppService;

    public CatalogAppService_Tests()
    {
        _externalProductCatalogClient = Substitute.For<IExternalProductCatalogClient>();
        _catalogAppService = new CatalogAppService(_externalProductCatalogClient);
    }

    [Fact]
    public async Task Should_Return_Found_With_Product_Data()
    {
        //Arrange
        _externalProductCatalogClient.GetByBarcodeAsync(Barcode).Returns(new ExternalProductDto
        {
            Barcode = Barcode,
            Name = "Nutella",
            Brand = "Nutella, Ferrero",
            ImageUrl = "https://images.openfoodfacts.org/nutella.jpg",
            NutriScore = "E",
            NovaGroup = 4,
            Allergens = new List<string> { "milk", "nuts", "soybeans" }
        });

        //Act
        var result = await _catalogAppService.GetByBarcodeAsync(new GetProductByBarcodeDto { Barcode = Barcode });

        //Assert
        result.Status.ShouldBe(ProductLookupStatus.Found);
        result.Barcode.ShouldBe(Barcode);
        result.Product.ShouldNotBeNull();
        result.Product.Name.ShouldBe("Nutella");
        result.Product.Brand.ShouldBe("Nutella, Ferrero");
        result.Product.ImageUrl.ShouldBe("https://images.openfoodfacts.org/nutella.jpg");
        result.Product.NutriScore.ShouldBe("E");
        result.Product.NovaGroup.ShouldBe(4);
        result.Product.Allergens.ShouldBe(new[] { "milk", "nuts", "soybeans" });
    }

    [Fact]
    public async Task Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        //Arrange
        _externalProductCatalogClient.GetByBarcodeAsync("0000000000000").Returns((ExternalProductDto?)null);

        //Act
        var result = await _catalogAppService.GetByBarcodeAsync(new GetProductByBarcodeDto { Barcode = "0000000000000" });

        //Assert
        result.Status.ShouldBe(ProductLookupStatus.NotFound);
        result.Barcode.ShouldBe("0000000000000");
        result.Product.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Keep_Missing_Fields_As_Null()
    {
        //Arrange - el proveedor no informa nombre, marca, imagen, puntajes ni alérgenos
        _externalProductCatalogClient.GetByBarcodeAsync(Barcode).Returns(new ExternalProductDto
        {
            Barcode = Barcode
        });

        //Act
        var result = await _catalogAppService.GetByBarcodeAsync(new GetProductByBarcodeDto { Barcode = Barcode });

        //Assert - no se inventan valores
        result.Status.ShouldBe(ProductLookupStatus.Found);
        result.Product.ShouldNotBeNull();
        result.Product.Barcode.ShouldBe(Barcode);
        result.Product.Name.ShouldBeNull();
        result.Product.Brand.ShouldBeNull();
        result.Product.ImageUrl.ShouldBeNull();
        result.Product.NutriScore.ShouldBeNull();
        result.Product.NovaGroup.ShouldBeNull();
        result.Product.Allergens.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Return_RateLimited_When_Provider_Limits_Requests()
    {
        //Arrange
        _externalProductCatalogClient.GetByBarcodeAsync(Barcode)
            .ThrowsAsync(new ExternalCatalogRateLimitException("HTTP 429"));

        //Act
        var result = await _catalogAppService.GetByBarcodeAsync(new GetProductByBarcodeDto { Barcode = Barcode });

        //Assert
        result.Status.ShouldBe(ProductLookupStatus.RateLimited);
        result.Product.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Return_Unavailable_When_Provider_Is_Down()
    {
        //Arrange
        _externalProductCatalogClient.GetByBarcodeAsync(Barcode)
            .ThrowsAsync(new ExternalCatalogUnavailableException("timeout"));

        //Act
        var result = await _catalogAppService.GetByBarcodeAsync(new GetProductByBarcodeDto { Barcode = Barcode });

        //Assert
        result.Status.ShouldBe(ProductLookupStatus.Unavailable);
        result.Product.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234567")]
    [InlineData("123456789012345")]
    [InlineData("30176204220AB")]
    public void Should_Reject_Invalid_Barcode(string barcode)
    {
        var input = new GetProductByBarcodeDto { Barcode = barcode };

        var isValid = Validator.TryValidateObject(input, new ValidationContext(input), new List<ValidationResult>(), true);

        isValid.ShouldBeFalse();
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("3017620422003")]
    [InlineData("12345678901234")]
    public void Should_Accept_Valid_Barcode(string barcode)
    {
        var input = new GetProductByBarcodeDto { Barcode = barcode };

        var isValid = Validator.TryValidateObject(input, new ValidationContext(input), new List<ValidationResult>(), true);

        isValid.ShouldBeTrue();
    }
}

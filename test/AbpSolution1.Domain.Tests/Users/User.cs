using System;
using Shouldly;
using Xunit;

namespace AbpSolution1.Users;

public class UserTests
{
    [Fact]
    public void Should_Create_A_Valid_User_And_Normalize_Data()
    {
        //Act
        var user = new User(
            Guid.NewGuid(),
            "  Juan Perez  ",
            "  JUAN@EXAMPLE.COM  ",
            "  Admin  "
        );

        //Assert
        user.DisplayName.ShouldBe("Juan Perez");
        user.Email.ShouldBe("juan@example.com");
        user.Role.ShouldBe("Admin");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_Not_Create_A_User_With_Empty_DisplayName(string invalidDisplayName)
    {
        Assert.Throws<ArgumentException>(() =>
        {
            new User(Guid.NewGuid(), invalidDisplayName, "juan@example.com", "Admin");
        });
    }
}
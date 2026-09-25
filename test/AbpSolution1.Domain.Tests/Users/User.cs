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

    [Fact]
    public void Should_Update_A_User_And_Normalize_Data()
    {
        //Arrange
        var user = new User(Guid.NewGuid(), "Juan Perez", "juan@example.com", "Admin");

        //Act
        user.Update("  Ana Gomez  ", "  ANA@EXAMPLE.COM  ", "  Member  ");

        //Assert
        user.DisplayName.ShouldBe("Ana Gomez");
        user.Email.ShouldBe("ana@example.com");
        user.Role.ShouldBe("Member");
    }

    [Theory]
    [InlineData("Ana Gomez", "ana@example.com", "")]
    [InlineData("Ana Gomez", "   ", "Member")]
    [InlineData(null, "ana@example.com", "Member")]
    public void Should_Reject_Invalid_Update_And_Keep_Previous_State(string displayName, string email, string role)
    {
        //Arrange
        var user = new User(Guid.NewGuid(), "Juan Perez", "juan@example.com", "Admin");

        //Act
        Assert.Throws<ArgumentException>(() =>
        {
            user.Update(displayName, email, role);
        });

        //Assert - ningún atributo quedó modificado parcialmente
        user.DisplayName.ShouldBe("Juan Perez");
        user.Email.ShouldBe("juan@example.com");
        user.Role.ShouldBe("Admin");
    }

    [Fact]
    public void Should_Reject_Update_That_Exceeds_Max_Length_And_Keep_Previous_State()
    {
        //Arrange
        var user = new User(Guid.NewGuid(), "Juan Perez", "juan@example.com", "Admin");
        var tooLongRole = new string('a', UserConsts.MaxRoleLength + 1);

        //Act
        Assert.Throws<ArgumentException>(() =>
        {
            user.Update("Ana Gomez", "ana@example.com", tooLongRole);
        });

        //Assert
        user.DisplayName.ShouldBe("Juan Perez");
        user.Email.ShouldBe("juan@example.com");
        user.Role.ShouldBe("Admin");
    }
}
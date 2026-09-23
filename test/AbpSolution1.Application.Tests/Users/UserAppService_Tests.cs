using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace AbpSolution1.Users;

public abstract class UserAppService_Tests<TStartupModule> : AbpSolution1ApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IUserAppService _userAppService;

    protected UserAppService_Tests()
    {
        _userAppService = GetRequiredService<IUserAppService>();
    }

    [Fact]
    public async Task Should_Create_And_Then_Get_User_By_Id()
    {
        //Act - Create
        var created = await _userAppService.CreateAsync(
            new CreateUserDto
            {
                DisplayName = "  Juan Perez  ",
                Email = "  JUAN@EXAMPLE.COM  ",
                Role = "Admin"
            }
        );

        //Act - Get
        var fetched = await _userAppService.GetAsync(created.Id);

        //Assert
        fetched.Id.ShouldBe(created.Id);
        fetched.DisplayName.ShouldBe("Juan Perez");
        fetched.Email.ShouldBe("juan@example.com");
        fetched.Role.ShouldBe("Admin");
    }

    [Fact]
    public async Task Should_Not_Create_A_User_Without_DisplayName()
    {
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _userAppService.CreateAsync(
                new CreateUserDto
                {
                    DisplayName = "",
                    Email = "juan@example.com",
                    Role = "Admin"
                }
            );
        });

        exception.ValidationErrors
            .ShouldContain(err => err.MemberNames.Any(mem => mem == "DisplayName"));
    }
}
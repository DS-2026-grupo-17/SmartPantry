using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
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

    [Fact]
    public async Task Should_Create_List_Update_Get_And_Delete_A_User()
    {
        //Registrar
        var created = await _userAppService.CreateAsync(
            new CreateUserDto
            {
                DisplayName = "Carla Diaz",
                Email = "carla@example.com",
                Role = "Member"
            }
        );

        //Listar
        var list = await _userAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        list.TotalCount.ShouldBeGreaterThan(0);
        list.Items.ShouldContain(u => u.Id == created.Id && u.DisplayName == "Carla Diaz");

        //Modificar
        var updated = await _userAppService.UpdateAsync(
            created.Id,
            new UpdateUserDto
            {
                DisplayName = "  Carla D. Diaz  ",
                Email = "  CARLA.DIAZ@EXAMPLE.COM  ",
                Role = "  Admin  "
            }
        );
        updated.DisplayName.ShouldBe("Carla D. Diaz");
        updated.Email.ShouldBe("carla.diaz@example.com");
        updated.Role.ShouldBe("Admin");

        //Consultar
        var fetched = await _userAppService.GetAsync(created.Id);
        fetched.DisplayName.ShouldBe("Carla D. Diaz");
        fetched.Email.ShouldBe("carla.diaz@example.com");
        fetched.Role.ShouldBe("Admin");

        //Eliminar y comprobar que el identificador ya no existe
        await _userAppService.DeleteAsync(created.Id);

        await Assert.ThrowsAnyAsync<EntityNotFoundException>(async () =>
        {
            await _userAppService.GetAsync(created.Id);
        });
    }

    [Fact]
    public async Task Should_Get_Paged_And_Sorted_List_Of_Users()
    {
        //Arrange
        await _userAppService.CreateAsync(new CreateUserDto { DisplayName = "Zoe", Email = "zoe@example.com", Role = "Member" });
        await _userAppService.CreateAsync(new CreateUserDto { DisplayName = "Bruno", Email = "bruno@example.com", Role = "Member" });
        await _userAppService.CreateAsync(new CreateUserDto { DisplayName = "Mario", Email = "mario@example.com", Role = "Member" });

        //Act
        var firstPage = await _userAppService.GetListAsync(
            new PagedAndSortedResultRequestDto { Sorting = "DisplayName", SkipCount = 0, MaxResultCount = 2 }
        );

        //Assert
        firstPage.TotalCount.ShouldBeGreaterThanOrEqualTo(3);
        firstPage.Items.Count.ShouldBe(2);
        firstPage.Items.Select(u => u.DisplayName).ShouldBeInOrder();
    }

    [Fact]
    public async Task Should_Not_Update_A_User_With_Invalid_Data()
    {
        //Arrange
        var created = await _userAppService.CreateAsync(
            new CreateUserDto
            {
                DisplayName = "Luis Ramos",
                Email = "luis@example.com",
                Role = "Member"
            }
        );

        //Act
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _userAppService.UpdateAsync(
                created.Id,
                new UpdateUserDto
                {
                    DisplayName = "Luis Ramos",
                    Email = "",
                    Role = "Admin"
                }
            );
        });

        //Assert
        exception.ValidationErrors
            .ShouldContain(err => err.MemberNames.Any(mem => mem == "Email"));

        var fetched = await _userAppService.GetAsync(created.Id);
        fetched.Email.ShouldBe("luis@example.com");
        fetched.Role.ShouldBe("Member");
    }
}

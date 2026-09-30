using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AbpSolution1.Users;

/*
 * GetAsync, GetListAsync (paginado y ordenable), CreateAsync, UpdateAsync y DeleteAsync
 * provienen de CrudAppService. Sólo se personaliza lo que involucra reglas del dominio:
 * - la creación usa el constructor de User (valida y normaliza) en lugar del mapeo DTO -> entidad;
 * - la modificación usa User.Update (valida todo antes de cambiar el estado);
 * - el orden por defecto de la lista es por DisplayName.
 */
public class UserAppService :
    CrudAppService<
        User,
        UserDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUserDto,
        UpdateUserDto>,
    IUserAppService
{
    public UserAppService(IRepository<User, Guid> repository)
        : base(repository)
    {
    }

    protected override Task<User> MapToEntityAsync(CreateUserDto createInput)
    {
        var user = new User(GuidGenerator.Create(), createInput.DisplayName, createInput.Email, createInput.Role);
        return Task.FromResult(user);
    }

    protected override Task MapToEntityAsync(UpdateUserDto updateInput, User entity)
    {
        entity.Update(updateInput.DisplayName, updateInput.Email, updateInput.Role);
        return Task.CompletedTask;
    }

    protected override IQueryable<User> ApplyDefaultSorting(IQueryable<User> query)
    {
        return query.OrderBy(user => user.DisplayName);
    }
}

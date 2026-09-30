using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AbpSolution1.Users;

public interface IUserAppService :
    ICrudAppService< //Define GetAsync, GetListAsync, CreateAsync, UpdateAsync y DeleteAsync
        UserDto, //Se usa para mostrar usuarios
        Guid, //Clave primaria de la entidad User
        PagedAndSortedResultRequestDto, //Se usa para paginar/ordenar la lista
        CreateUserDto, //Se usa para registrar un usuario
        UpdateUserDto> //Se usa para modificar un usuario
{
}

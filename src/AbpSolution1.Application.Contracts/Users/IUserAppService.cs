using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace AbpSolution1.Users;

public interface IUserAppService : IApplicationService
{
    Task<UserDto> GetAsync(Guid id);
    Task<UserDto> CreateAsync(CreateUserDto input);
}
using System;
using Volo.Abp.Application.Dtos;

namespace AbpSolution1.Users;

public class UserDto : EntityDto<Guid>
{
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}

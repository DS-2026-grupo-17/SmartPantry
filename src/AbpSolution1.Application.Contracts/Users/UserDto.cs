using System;

namespace AbpSolution1.Users;

public class UserDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}
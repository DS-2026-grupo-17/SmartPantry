using System.ComponentModel.DataAnnotations;

namespace AbpSolution1.Users;

public class CreateUserDto
{
    [Required]
    [StringLength(UserConsts.MaxDisplayNameLength)]
    public string DisplayName { get; set; }

    [Required]
    [StringLength(UserConsts.MaxEmailLength)]
    public string Email { get; set; }

    [Required]
    [StringLength(UserConsts.MaxRoleLength)]
    public string Role { get; set; }
}
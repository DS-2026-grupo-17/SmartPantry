using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace AbpSolution1.Users;

public class User : AggregateRoot<Guid>
{
    public string DisplayName { get; private set; }
    public string Email { get; private set; }
    public string Role { get; private set; }

    protected User() { }

    public User(Guid id, string displayName, string email, string role) : base(id)
    {
        DisplayName = Check.NotNullOrWhiteSpace(displayName, nameof(displayName), UserConsts.MaxDisplayNameLength).Trim();
        Email = Check.NotNullOrWhiteSpace(email, nameof(email), UserConsts.MaxEmailLength).Trim().ToLower();
        Role = Check.NotNullOrWhiteSpace(role, nameof(role), UserConsts.MaxRoleLength).Trim();
    }
}
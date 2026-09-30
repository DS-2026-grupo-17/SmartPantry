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
        DisplayName = NormalizeDisplayName(displayName);
        Email = NormalizeEmail(email);
        Role = NormalizeRole(role);
    }

    public void Update(string displayName, string email, string role)
    {
        // Se validan y normalizan todos los valores antes de asignar:
        // si alguno es inválido se lanza la excepción sin modificar el estado.
        var normalizedDisplayName = NormalizeDisplayName(displayName);
        var normalizedEmail = NormalizeEmail(email);
        var normalizedRole = NormalizeRole(role);

        DisplayName = normalizedDisplayName;
        Email = normalizedEmail;
        Role = normalizedRole;
    }

    private static string NormalizeDisplayName(string displayName)
    {
        return Check.NotNullOrWhiteSpace(displayName, nameof(displayName), UserConsts.MaxDisplayNameLength).Trim();
    }

    private static string NormalizeEmail(string email)
    {
        return Check.NotNullOrWhiteSpace(email, nameof(email), UserConsts.MaxEmailLength).Trim().ToLower();
    }

    private static string NormalizeRole(string role)
    {
        return Check.NotNullOrWhiteSpace(role, nameof(role), UserConsts.MaxRoleLength).Trim();
    }
}

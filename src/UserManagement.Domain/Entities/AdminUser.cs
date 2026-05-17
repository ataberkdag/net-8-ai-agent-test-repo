using UserManagement.Domain.Enums;

namespace UserManagement.Domain.Entities;

public sealed class AdminUser : User
{
    public AdminUser(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string permissionLevel,
        DateTime createdAtUtc)
        : base(id, firstName, lastName, email, UserType.Admin, createdAtUtc)
    {
        PermissionLevel = permissionLevel;
    }

    public string PermissionLevel { get; }
}

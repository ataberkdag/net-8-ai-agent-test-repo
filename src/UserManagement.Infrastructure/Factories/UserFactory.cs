using UserManagement.Application.Abstractions;
using UserManagement.Application.Models;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;

namespace UserManagement.Infrastructure.Factories;

public sealed class UserFactory : IUserFactory
{
    public User Create(CreateUserCommand command)
    {
        var id = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        return command.UserType switch
        {
            UserType.Admin => new AdminUser(
                id,
                command.FirstName.Trim(),
                command.LastName.Trim(),
                command.Email.Trim(),
                command.PermissionLevel!.Trim(),
                createdAtUtc),
            UserType.Customer => new CustomerUser(
                id,
                command.FirstName.Trim(),
                command.LastName.Trim(),
                command.Email.Trim(),
                command.LoyaltyCode!.Trim(),
                createdAtUtc),
            UserType.Employee => new EmployeeUser(
                id,
                command.FirstName.Trim(),
                command.LastName.Trim(),
                command.Email.Trim(),
                command.Department!.Trim(),
                createdAtUtc),
            _ => throw new InvalidOperationException("Unsupported user type.")
        };
    }
}

using UserManagement.Application.Abstractions;
using UserManagement.Application.Models;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;

namespace UserManagement.Infrastructure.Factories;

public sealed class UserFactory : IUserFactory
{
    public User Create(CreateUserCommand command)
    {
        return command.UserType switch
        {
            UserType.Admin => new AdminUser(
                Guid.NewGuid(),
                command.FirstName,
                command.LastName,
                command.Email,
                command.PermissionLevel!,
                DateTime.UtcNow),
            UserType.Customer => new CustomerUser(
                Guid.NewGuid(),
                command.FirstName,
                command.LastName,
                command.Email,
                command.LoyaltyCode!,
                DateTime.UtcNow),
            UserType.Employee => new EmployeeUser(
                Guid.NewGuid(),
                command.FirstName,
                command.LastName,
                command.Email,
                command.Department!,
                DateTime.UtcNow),
            _ => throw new InvalidOperationException("Unsupported user type.")
        };
    }
}

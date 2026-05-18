using UserManagement.Application.Models;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Abstractions;

public interface IUserFactory
{
    User Create(CreateUserCommand command);

    User Create(UpdateUserCommand command, DateTime createdAtUtc);
}

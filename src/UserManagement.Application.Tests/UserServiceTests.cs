using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Application.Abstractions;
using UserManagement.Application.Exceptions;
using UserManagement.Application.Models;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using Xunit;

namespace UserManagement.Application.Tests;

public sealed class UserServiceTests
{
    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedUser_WhenUserExists()
    {
        var existingUser = new AdminUser("Existing", "User", "existing@example.com", "L1")
        {
            Id = Guid.NewGuid(),
            CreatedAtUtc = new DateTime(2024, 01, 01, 00, 00, 00, DateTimeKind.Utc)
        };

        var repository = new TestUserRepository(existingUser);
        var service = new UserService(new TestUserFactory(), repository);

        var result = await service.UpdateAsync(new UpdateUserCommand(
            existingUser.Id,
            "Updated",
            "User",
            "updated@example.com",
            UserType.Admin,
            "L2",
            null,
            null), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(existingUser.Id, result!.Id);
        Assert.Equal("Updated", result.FirstName);
        Assert.Equal("L2", result.PermissionLevel);
        Assert.Equal(existingUser.CreatedAtUtc, result.CreatedAtUtc);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        var repository = new TestUserRepository(null);
        var service = new UserService(new TestUserFactory(), repository);

        var result = await service.UpdateAsync(new UpdateUserCommand(
            Guid.NewGuid(),
            "Updated",
            "User",
            "updated@example.com",
            UserType.Admin,
            "L2",
            null,
            null), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsValidationException_WhenInvalidEmail()
    {
        var repository = new TestUserRepository(null);
        var service = new UserService(new TestUserFactory(), repository);

        await Assert.ThrowsAsync<UserValidationException>(() => service.UpdateAsync(new UpdateUserCommand(
            Guid.NewGuid(),
            "Updated",
            "User",
            "invalid-email",
            UserType.Admin,
            "L2",
            null,
            null), CancellationToken.None));
    }

    private sealed class TestUserFactory : IUserFactory
    {
        public User Create(CreateUserCommand command)
        {
            return command.UserType switch
            {
                UserType.Admin => new AdminUser(command.FirstName, command.LastName, command.Email, command.PermissionLevel ?? string.Empty),
                UserType.Customer => new CustomerUser(command.FirstName, command.LastName, command.Email, command.LoyaltyCode ?? string.Empty),
                UserType.Employee => new EmployeeUser(command.FirstName, command.LastName, command.Email, command.Department ?? string.Empty),
                _ => throw new InvalidOperationException()
            };
        }
    }

    private sealed class TestUserRepository : IUserRepository
    {
        private User? _user;

        public TestUserRepository(User? user)
        {
            _user = user;
        }

        public Task<User> AddAsync(User user, CancellationToken cancellationToken)
        {
            _user = user;
            return Task.FromResult(user);
        }

        public Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken)
        {
            IReadOnlyCollection<User> users = _user is null ? [] : [_user];
            return Task.FromResult(users);
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_user is not null && _user.Id == id ? _user : null);
        }

        public Task<User?> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            if (_user is null || _user.Id != user.Id)
            {
                return Task.FromResult<User?>(null);
            }

            _user = user;
            return Task.FromResult<User?>(user);
        }
    }
}

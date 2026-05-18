using System;
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
        var userId = Guid.NewGuid();
        var createdAtUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var existingUser = new AdminUser(userId, "Old", "Name", "old@example.com", "Full", createdAtUtc);
        var repository = new FakeUserRepository(existingUser);
        var service = new UserService(new FakeUserFactory(), repository);

        var result = await service.UpdateAsync(
            userId,
            new UpdateUserCommand("New", "Name", "new@example.com", UserType.Admin, "Admin", null, null),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(userId, result!.Id);
        Assert.Equal("New", result.FirstName);
        Assert.Equal("new@example.com", result.Email);
        Assert.Equal("Admin", result.PermissionLevel);
        Assert.Equal(createdAtUtc, result.CreatedAtUtc);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        var repository = new FakeUserRepository(null);
        var service = new UserService(new FakeUserFactory(), repository);

        var result = await service.UpdateAsync(
            Guid.NewGuid(),
            new UpdateUserCommand("New", "Name", "new@example.com", UserType.Admin, "Admin", null, null),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsValidationException_ForInvalidEmail()
    {
        var repository = new FakeUserRepository(new AdminUser(Guid.NewGuid(), "Old", "Name", "old@example.com", "Full", DateTime.UtcNow));
        var service = new UserService(new FakeUserFactory(), repository);

        var exception = await Assert.ThrowsAsync<UserValidationException>(async () =>
            await service.UpdateAsync(
                Guid.NewGuid(),
                new UpdateUserCommand("New", "Name", "invalid-email", UserType.Admin, "Admin", null, null),
                CancellationToken.None));

        Assert.Equal("Email format is invalid.", exception.Message);
    }

    private sealed class FakeUserFactory : IUserFactory
    {
        public User Create(CreateUserCommand command)
        {
            return command.UserType switch
            {
                UserType.Admin => new AdminUser(Guid.NewGuid(), command.FirstName, command.LastName, command.Email, command.PermissionLevel!, DateTime.UtcNow),
                UserType.Customer => new CustomerUser(Guid.NewGuid(), command.FirstName, command.LastName, command.Email, command.LoyaltyCode!, DateTime.UtcNow),
                UserType.Employee => new EmployeeUser(Guid.NewGuid(), command.FirstName, command.LastName, command.Email, command.Department!, DateTime.UtcNow),
                _ => throw new InvalidOperationException()
            };
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private User? _user;

        public FakeUserRepository(User? user)
        {
            _user = user;
        }

        public Task<User> AddAsync(User user, CancellationToken cancellationToken)
        {
            _user = user;
            return Task.FromResult(user);
        }

        public Task<User?> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            if (_user is null)
            {
                return Task.FromResult<User?>(null);
            }

            _user = user;
            return Task.FromResult<User?>(user);
        }

        public Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken)
        {
            IReadOnlyCollection<User> users = _user is null ? Array.Empty<User>() : new[] { _user };
            return Task.FromResult(users);
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_user is not null && _user.Id == id ? _user : null);
        }
    }
}

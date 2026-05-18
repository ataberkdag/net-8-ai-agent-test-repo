using System;
using System.Collections.Generic;
using System.Linq;
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
    public async Task UpdateAsync_WhenUserExists_UpdatesUserAndPreservesIdAndCreatedAtUtc()
    {
        var userId = Guid.NewGuid();
        var createdAtUtc = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var repository = new TestUserRepository();
        repository.Seed(new CustomerUser(
            userId,
            "Old",
            "Customer",
            "old.customer@example.com",
            "OLD-LOYALTY",
            createdAtUtc));
        var service = new UserService(new TestUserFactory(), repository);
        var command = new UpdateUserCommand(
            userId,
            "Updated",
            "Employee",
            "updated.employee@example.com",
            UserType.Employee,
            null,
            null,
            "Engineering");

        var result = await service.UpdateAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal(createdAtUtc, result.CreatedAtUtc);
        Assert.Equal("Updated", result.FirstName);
        Assert.Equal("Employee", result.LastName);
        Assert.Equal("updated.employee@example.com", result.Email);
        Assert.Equal(UserType.Employee, result.UserType);
        Assert.Equal("Engineering", result.Department);
        Assert.Null(result.PermissionLevel);
        Assert.Null(result.LoyaltyCode);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        var repository = new TestUserRepository();
        var service = new UserService(new TestUserFactory(), repository);
        var command = new UpdateUserCommand(
            Guid.NewGuid(),
            "Missing",
            "User",
            "missing.user@example.com",
            UserType.Customer,
            null,
            "LOYALTY-1",
            null);

        var result = await service.UpdateAsync(command, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenTypeSpecificFieldIsMissing_ThrowsUserValidationException()
    {
        var repository = new TestUserRepository();
        var service = new UserService(new TestUserFactory(), repository);
        var command = new UpdateUserCommand(
            Guid.NewGuid(),
            "Admin",
            "User",
            "admin.user@example.com",
            UserType.Admin,
            null,
            null,
            null);

        var exception = await Assert.ThrowsAsync<UserValidationException>(() => service.UpdateAsync(command, CancellationToken.None));

        Assert.Equal("PermissionLevel is required for admin users.", exception.Message);
    }

    private sealed class TestUserFactory : IUserFactory
    {
        public User Create(CreateUserCommand command)
        {
            var createdAtUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return command.UserType switch
            {
                UserType.Admin => new AdminUser(
                    Guid.NewGuid(),
                    command.FirstName,
                    command.LastName,
                    command.Email,
                    command.PermissionLevel!,
                    createdAtUtc),
                UserType.Customer => new CustomerUser(
                    Guid.NewGuid(),
                    command.FirstName,
                    command.LastName,
                    command.Email,
                    command.LoyaltyCode!,
                    createdAtUtc),
                UserType.Employee => new EmployeeUser(
                    Guid.NewGuid(),
                    command.FirstName,
                    command.LastName,
                    command.Email,
                    command.Department!,
                    createdAtUtc),
                _ => throw new InvalidOperationException("Unsupported user type.")
            };
        }
    }

    private sealed class TestUserRepository : IUserRepository
    {
        private readonly Dictionary<Guid, User> _users = new();

        public void Seed(User user)
        {
            _users[user.Id] = user;
        }

        public Task<User> AddAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _users[user.Id] = user;

            return Task.FromResult(user);
        }

        public Task<User?> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_users.ContainsKey(user.Id))
            {
                return Task.FromResult<User?>(null);
            }

            _users[user.Id] = user;

            return Task.FromResult<User?>(user);
        }

        public Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyCollection<User> users = _users.Values
                .OrderBy(user => user.CreatedAtUtc)
                .ToArray();

            return Task.FromResult(users);
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _users.TryGetValue(id, out var user);

            return Task.FromResult(user);
        }
    }
}

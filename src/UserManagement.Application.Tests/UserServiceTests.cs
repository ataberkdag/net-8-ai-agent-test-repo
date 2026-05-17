using System;
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
    public async Task UpdateAsync_returns_not_found_when_user_does_not_exist()
    {
        var userFactory = new TestUserFactory();
        var repository = new TestUserRepository();
        var service = new UserService(userFactory, repository);

        var result = await service.UpdateAsync(
            Guid.NewGuid(),
            new CreateUserCommand("Jane", "Doe", "jane@example.com", UserType.Customer, null, "LOY-1", null),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_updates_existing_user_and_preserves_created_time()
    {
        var id = Guid.NewGuid();
        var createdAt = new DateTime(2024, 01, 01, 12, 00, 00, DateTimeKind.Utc);
        var existingUser = new CustomerUser(id, "Old", "Name", "old@example.com", createdAt, "OLD-1");

        var userFactory = new TestUserFactory();
        var repository = new TestUserRepository(existingUser);
        var service = new UserService(userFactory, repository);

        var result = await service.UpdateAsync(
            id,
            new CreateUserCommand("Jane", "Doe", "jane@example.com", UserType.Customer, null, "LOY-1", null),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.Equal(createdAt, result.CreatedAtUtc);
        Assert.Equal("Jane", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("jane@example.com", result.Email);
        Assert.Equal("LOY-1", result.LoyaltyCode);
    }

    [Fact]
    public async Task UpdateAsync_throws_validation_exception_for_invalid_request()
    {
        var userFactory = new TestUserFactory();
        var repository = new TestUserRepository();
        var service = new UserService(userFactory, repository);

        var exception = await Assert.ThrowsAsync<UserValidationException>(() =>
            service.UpdateAsync(
                Guid.NewGuid(),
                new CreateUserCommand(string.Empty, "Doe", "jane@example.com", UserType.Customer, null, "LOY-1", null),
                CancellationToken.None));

        Assert.Equal("FirstName is required.", exception.Message);
    }

    private sealed class TestUserFactory : IUserFactory
    {
        public User Create(CreateUserCommand command)
        {
            return command.UserType switch
            {
                UserType.Admin => new AdminUser(Guid.NewGuid(), command.FirstName, command.LastName, command.Email, DateTime.UtcNow, command.PermissionLevel ?? string.Empty),
                UserType.Customer => new CustomerUser(Guid.NewGuid(), command.FirstName, command.LastName, command.Email, DateTime.UtcNow, command.LoyaltyCode ?? string.Empty),
                UserType.Employee => new EmployeeUser(Guid.NewGuid(), command.FirstName, command.LastName, command.Email, DateTime.UtcNow, command.Department ?? string.Empty),
                _ => throw new InvalidOperationException()
            };
        }
    }

    private sealed class TestUserRepository : IUserRepository
    {
        private readonly Dictionary<Guid, User> _users = new();

        public TestUserRepository(params User[] users)
        {
            foreach (var user in users)
            {
                _users[user.Id] = user;
            }
        }

        public Task<User> AddAsync(User user, CancellationToken cancellationToken)
        {
            _users[user.Id] = user;
            return Task.FromResult(user);
        }

        public Task<User> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            _users[user.Id] = user;
            return Task.FromResult(user);
        }

        public Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken)
        {
            IReadOnlyCollection<User> users = _users.Values.ToArray();
            return Task.FromResult(users);
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Application.Abstractions;
using UserManagement.Application.Models;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using Xunit;

namespace UserManagement.Application.Tests;

public sealed class UserServiceTests
{
    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        var userFactory = new FakeUserFactory();
        var userRepository = new FakeUserRepository();
        var service = new UserService(userFactory, userRepository);

        var command = new UpdateUserCommand(
            Guid.NewGuid(),
            "Jane",
            "Doe",
            "jane.doe@example.com",
            UserType.Admin,
            "Super",
            null,
            null);

        var result = await service.UpdateAsync(command, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingUser_AndPreservesCreatedAtUtc()
    {
        var userFactory = new FakeUserFactory();
        var userRepository = new FakeUserRepository();
        var service = new UserService(userFactory, userRepository);
        var createdAtUtc = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var userId = Guid.NewGuid();

        await userRepository.AddAsync(
            new AdminUser(
                userId,
                "Old",
                "Name",
                "old@example.com",
                "Basic",
                createdAtUtc),
            CancellationToken.None);

        var command = new UpdateUserCommand(
            userId,
            "Jane",
            "Doe",
            "jane.doe@example.com",
            UserType.Employee,
            null,
            null,
            "Engineering");

        var result = await service.UpdateAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("Jane", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("jane.doe@example.com", result.Email);
        Assert.Equal(UserType.Employee, result.UserType);
        Assert.Equal(createdAtUtc, result.CreatedAtUtc);
        Assert.Null(result.PermissionLevel);
        Assert.Null(result.LoyaltyCode);
        Assert.Equal("Engineering", result.Department);
    }

    private sealed class FakeUserFactory : IUserFactory
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
                _ => throw new InvalidOperationException()
            };
        }

        public User Create(UpdateUserCommand command, DateTime createdAtUtc)
        {
            return command.UserType switch
            {
                UserType.Admin => new AdminUser(
                    command.Id,
                    command.FirstName,
                    command.LastName,
                    command.Email,
                    command.PermissionLevel!,
                    createdAtUtc),
                UserType.Customer => new CustomerUser(
                    command.Id,
                    command.FirstName,
                    command.LastName,
                    command.Email,
                    command.LoyaltyCode!,
                    createdAtUtc),
                UserType.Employee => new EmployeeUser(
                    command.Id,
                    command.FirstName,
                    command.LastName,
                    command.Email,
                    command.Department!,
                    createdAtUtc),
                _ => throw new InvalidOperationException()
            };
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly System.Collections.Generic.Dictionary<Guid, User> _users = new();

        public Task<User> AddAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _users[user.Id] = user;
            return Task.FromResult(user);
        }

        public Task<System.Collections.Generic.IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            System.Collections.Generic.IReadOnlyCollection<User> users = _users.Values.ToArray();
            return Task.FromResult(users);
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _users.TryGetValue(id, out var user);
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
    }
}

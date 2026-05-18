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

namespace UserManagement.Application.Tests.Services;

public sealed class UserServiceTests
{
    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedUser_WhenUserExists()
    {
        var existingUser = new AdminUser(
            Guid.NewGuid(),
            "John",
            "Doe",
            "john@example.com",
            "Read",
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var updatedUser = new AdminUser(
            existingUser.Id,
            "Jane",
            "Smith",
            "jane@example.com",
            "Write",
            existingUser.CreatedAtUtc);

        var repository = new FakeUserRepository(existingUser, updatedUser);
        var factory = new FakeUserFactory(updatedUser);
        var service = new UserService(factory, repository);

        var command = new UpdateUserCommand(
            existingUser.Id,
            "Jane",
            "Smith",
            "jane@example.com",
            UserType.Admin,
            "Write",
            null,
            null);

        var result = await service.UpdateAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(existingUser.Id, result.Id);
        Assert.Equal("Jane", result.FirstName);
        Assert.Equal("Smith", result.LastName);
        Assert.Equal("jane@example.com", result.Email);
        Assert.Equal("Write", result.PermissionLevel);
        Assert.Equal(existingUser.CreatedAtUtc, result.CreatedAtUtc);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        var repository = new FakeUserRepository(null, null);
        var factory = new FakeUserFactory(new AdminUser(
            Guid.NewGuid(),
            "Jane",
            "Smith",
            "jane@example.com",
            "Write",
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        var service = new UserService(factory, repository);

        var command = new UpdateUserCommand(
            Guid.NewGuid(),
            "Jane",
            "Smith",
            "jane@example.com",
            UserType.Admin,
            "Write",
            null,
            null);

        var result = await service.UpdateAsync(command, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsValidationException_WhenAdminPermissionLevelMissing()
    {
        var repository = new FakeUserRepository(null, null);
        var factory = new FakeUserFactory(new AdminUser(
            Guid.NewGuid(),
            "Jane",
            "Smith",
            "jane@example.com",
            "Write",
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        var service = new UserService(factory, repository);

        var command = new UpdateUserCommand(
            Guid.NewGuid(),
            "Jane",
            "Smith",
            "jane@example.com",
            UserType.Admin,
            null,
            null,
            null);

        await Assert.ThrowsAsync<UserValidationException>(() => service.UpdateAsync(command, CancellationToken.None));
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly User? _existingUser;
        private readonly User? _updatedUser;

        public FakeUserRepository(User? existingUser, User? updatedUser)
        {
            _existingUser = existingUser;
            _updatedUser = updatedUser;
        }

        public Task<User> AddAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken)
        {
            IReadOnlyCollection<User> users = Array.Empty<User>();
            return Task.FromResult(users);
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_existingUser is not null && _existingUser.Id == id ? _existingUser : null);
        }

        public Task<User?> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(_updatedUser);
        }
    }

    private sealed class FakeUserFactory : IUserFactory
    {
        private readonly User _user;

        public FakeUserFactory(User user)
        {
            _user = user;
        }

        public User Create(CreateUserCommand command)
        {
            return _user;
        }

        public User Create(UpdateUserCommand command, DateTime createdAtUtc)
        {
            return _user;
        }
    }
}

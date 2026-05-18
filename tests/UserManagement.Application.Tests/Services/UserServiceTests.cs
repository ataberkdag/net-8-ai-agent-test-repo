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
    public async Task UpdateAsync_WhenUserExists_ReturnsUpdatedUserResponse()
    {
        var userId = Guid.NewGuid();
        var createdAtUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var existingUser = new CustomerUser(userId, "Jane", "Doe", "jane@example.com", "LOYAL-1", createdAtUtc);
        var updatedUser = new EmployeeUser(userId, "John", "Smith", "john@example.com", "IT", createdAtUtc);
        var factory = new FakeUserFactory(updatedUser);
        var repository = new FakeUserRepository(existingUser, updatedUser);
        var service = new UserService(factory, repository);
        var command = new UpdateUserCommand(userId, "John", "Smith", "john@example.com", UserType.Employee, null, null, "IT");

        var result = await service.UpdateAsync(command, CancellationToken.None);

        Assert.Equal(userId, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Smith", result.LastName);
        Assert.Equal("john@example.com", result.Email);
        Assert.Equal(UserType.Employee, result.UserType);
        Assert.Equal(createdAtUtc, result.CreatedAtUtc);
        Assert.Equal("IT", result.Department);
        Assert.Null(result.PermissionLevel);
        Assert.Null(result.LoyaltyCode);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserDoesNotExist_ThrowsUserNotFoundException()
    {
        var userId = Guid.NewGuid();
        var factory = new FakeUserFactory(new AdminUser(userId, "John", "Smith", "john@example.com", "Full", DateTime.UtcNow));
        var repository = new FakeUserRepository(null, null);
        var service = new UserService(factory, repository);
        var command = new UpdateUserCommand(userId, "John", "Smith", "john@example.com", UserType.Admin, "Full", null, null);

        await Assert.ThrowsAsync<UserNotFoundException>(() => service.UpdateAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_WhenCommandIsInvalid_ThrowsUserValidationException()
    {
        var userId = Guid.NewGuid();
        var factory = new FakeUserFactory(new AdminUser(userId, "John", "Smith", "john@example.com", "Full", DateTime.UtcNow));
        var repository = new FakeUserRepository(null, null);
        var service = new UserService(factory, repository);
        var command = new UpdateUserCommand(userId, "John", "Smith", "invalid-email", UserType.Admin, "Full", null, null);

        await Assert.ThrowsAsync<UserValidationException>(() => service.UpdateAsync(command, CancellationToken.None));
    }

    private sealed class FakeUserFactory : IUserFactory
    {
        private readonly User _updatedUser;

        public FakeUserFactory(User updatedUser)
        {
            _updatedUser = updatedUser;
        }

        public User Create(CreateUserCommand command)
        {
            throw new NotSupportedException();
        }

        public User Create(UpdateUserCommand command, DateTime createdAtUtc)
        {
            return _updatedUser;
        }
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
            throw new NotSupportedException();
        }

        public Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken)
        {
            IReadOnlyCollection<User> users = Array.Empty<User>();
            return Task.FromResult(users);
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_existingUser);
        }

        public Task<User?> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(_updatedUser);
        }
    }
}

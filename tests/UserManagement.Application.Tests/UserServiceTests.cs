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
        var userRepository = new FakeUserRepository();
        var userFactory = new FakeUserFactory();
        var service = new UserService(userFactory, userRepository);
        var existingUser = new AdminUser(Guid.NewGuid(), "Old", "Name", "old@example.com", "Admin", DateTime.UtcNow.AddMinutes(-5));

        await userRepository.AddAsync(existingUser, CancellationToken.None);

        var result = await service.UpdateAsync(
            existingUser.Id,
            new UpdateUserCommand(
                "New",
                "Name",
                "new@example.com",
                UserType.Admin,
                "Super",
                null,
                null),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(existingUser.Id, result!.Id);
        Assert.Equal("New", result.FirstName);
        Assert.Equal("new@example.com", result.Email);
        Assert.Equal("Super", result.PermissionLevel);
        Assert.Equal(existingUser.CreatedAtUtc, result.CreatedAtUtc);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        var userRepository = new FakeUserRepository();
        var userFactory = new FakeUserFactory();
        var service = new UserService(userFactory, userRepository);

        var result = await service.UpdateAsync(
            Guid.NewGuid(),
            new UpdateUserCommand(
                "New",
                "Name",
                "new@example.com",
                UserType.Admin,
                "Super",
                null,
                null),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsValidationException_WhenEmailIsInvalid()
    {
        var userRepository = new FakeUserRepository();
        var userFactory = new FakeUserFactory();
        var service = new UserService(userFactory, userRepository);
        var existingUser = new AdminUser(Guid.NewGuid(), "Old", "Name", "old@example.com", "Admin", DateTime.UtcNow);

        await userRepository.AddAsync(existingUser, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<UserValidationException>(() => service.UpdateAsync(
            existingUser.Id,
            new UpdateUserCommand(
                "New",
                "Name",
                "invalid-email",
                UserType.Admin,
                "Super",
                null,
                null),
            CancellationToken.None));

        Assert.Equal("Email format is invalid.", exception.Message);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly Dictionary<Guid, User> _users = new();

        public Task<User> AddAsync(User user, CancellationToken cancellationToken)
        {
            _users[user.Id] = user;
            return Task.FromResult(user);
        }

        public Task<User?> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            if (!_users.ContainsKey(user.Id))
            {
                return Task.FromResult<User?>(null);
            }

            _users[user.Id] = user;
            return Task.FromResult<User?>(user);
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

    private sealed class FakeUserFactory : IUserFactory
    {
        public User Create(CreateUserCommand command)
        {
            return command.UserType switch
            {
                UserType.Admin => new AdminUser(Guid.NewGuid(), command.FirstName, command.LastName, command.Email, command.PermissionLevel ?? string.Empty, DateTime.UtcNow),
                UserType.Customer => new CustomerUser(Guid.NewGuid(), command.FirstName, command.LastName, command.Email, command.LoyaltyCode ?? string.Empty, DateTime.UtcNow),
                UserType.Employee => new EmployeeUser(Guid.NewGuid(), command.FirstName, command.LastName, command.Email, command.Department ?? string.Empty, DateTime.UtcNow),
                _ => throw new InvalidOperationException()
            };
        }
    }
}

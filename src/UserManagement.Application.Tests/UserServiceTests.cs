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
    public async Task UpdateAsync_returns_updated_user_response_for_existing_user()
    {
        var userId = Guid.NewGuid();
        var createdAtUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var existingUser = new AdminUser(userId, "Old", "Name", "old@example.com", "Level 1", createdAtUtc);
        var repository = new FakeUserRepository(existingUser);
        var service = CreateService(repository);

        var result = await service.UpdateAsync(
            userId,
            new UpdateUserCommand("New", "Name", "new@example.com", UserType.Admin, "Level 2", null, null),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(userId, result!.Id);
        Assert.Equal("New", result.FirstName);
        Assert.Equal("Name", result.LastName);
        Assert.Equal("new@example.com", result.Email);
        Assert.Equal(UserType.Admin, result.UserType);
        Assert.Equal("Level 2", result.PermissionLevel);
        Assert.Equal(createdAtUtc, result.CreatedAtUtc);
    }

    [Fact]
    public async Task UpdateAsync_returns_null_when_user_does_not_exist()
    {
        var repository = new FakeUserRepository();
        var service = CreateService(repository);

        var result = await service.UpdateAsync(
            Guid.NewGuid(),
            new UpdateUserCommand("New", "Name", "new@example.com", UserType.Admin, "Level 2", null, null),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_throws_validation_exception_for_invalid_email()
    {
        var repository = new FakeUserRepository();
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<UserValidationException>(() =>
            service.UpdateAsync(
                Guid.NewGuid(),
                new UpdateUserCommand("New", "Name", "invalid-email", UserType.Admin, "Level 2", null, null),
                CancellationToken.None));

        Assert.Equal("Email format is invalid.", exception.Message);
    }

    private static UserService CreateService(FakeUserRepository repository)
    {
        return new UserService(new FakeUserFactory(), repository);
    }

    private sealed class FakeUserFactory : IUserFactory
    {
        public User Create(CreateUserCommand command)
        {
            return new AdminUser(Guid.NewGuid(), command.FirstName, command.LastName, command.Email, command.PermissionLevel ?? string.Empty, DateTime.UtcNow);
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private User? _user;

        public FakeUserRepository(User? user = null)
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

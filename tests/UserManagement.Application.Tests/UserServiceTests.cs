using UserManagement.Application.Models;
using UserManagement.Application.Services;
using UserManagement.Infrastructure.Factories;
using UserManagement.Infrastructure.Repositories;
using Xunit;

namespace UserManagement.Application.Tests;

public sealed class UserServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidAdminCommand_CreatesUser()
    {
        var service = new UserService(new UserFactory(), new InMemoryUserRepository());
        var command = new CreateUserCommand(
            "Alice",
            "Admin",
            "alice.admin@example.com",
            UserManagement.Domain.Enums.UserType.Admin,
            "Full",
            null,
            null);

        var result = await service.CreateAsync(command, CancellationToken.None);

        Assert.Equal("Alice", result.FirstName);
        Assert.Equal("Admin", result.LastName);
        Assert.Equal("alice.admin@example.com", result.Email);
        Assert.Equal(UserManagement.Domain.Enums.UserType.Admin, result.UserType);
        Assert.Equal("Full", result.PermissionLevel);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCreatedUsers()
    {
        var repository = new InMemoryUserRepository();
        var service = new UserService(new UserFactory(), repository);

        await service.CreateAsync(
            new CreateUserCommand(
                "Alice",
                "Admin",
                "alice.admin@example.com",
                UserManagement.Domain.Enums.UserType.Admin,
                "Full",
                null,
                null),
            CancellationToken.None);

        var result = await service.GetAllAsync(CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullWhenUserDoesNotExist()
    {
        var service = new UserService(new UserFactory(), new InMemoryUserRepository());

        var result = await service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Application.Exceptions;
using UserManagement.Application.Models;
using UserManagement.Application.Services;
using UserManagement.Domain.Enums;
using UserManagement.Infrastructure.Factories;
using UserManagement.Infrastructure.Repositories;
using Xunit;

namespace UserManagement.Tests;

public sealed class UserServiceTests
{
    [Fact]
    public async Task UpdateAsync_WhenUserExists_UpdatesUserAndPreservesCreatedAtUtc()
    {
        var service = CreateService();
        var created = await service.CreateAsync(
            new CreateUserCommand(
                "John",
                "Smith",
                "john.smith@example.com",
                UserType.Customer,
                null,
                "LOYAL-1",
                null),
            CancellationToken.None);

        var updated = await service.UpdateAsync(
            new UpdateUserCommand(
                created.Id,
                "Jane",
                "Doe",
                "jane.doe@example.com",
                UserType.Admin,
                "Full",
                null,
                null),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated!.Id);
        Assert.Equal(created.CreatedAtUtc, updated.CreatedAtUtc);
        Assert.Equal("Jane", updated.FirstName);
        Assert.Equal("Doe", updated.LastName);
        Assert.Equal("jane.doe@example.com", updated.Email);
        Assert.Equal(UserType.Admin, updated.UserType);
        Assert.Equal("Full", updated.PermissionLevel);
        Assert.Null(updated.LoyaltyCode);
        Assert.Null(updated.Department);

        var stored = await service.GetByIdAsync(created.Id, CancellationToken.None);
        Assert.Equal(updated, stored);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        var service = CreateService();

        var updated = await service.UpdateAsync(
            new UpdateUserCommand(
                Guid.NewGuid(),
                "Jane",
                "Doe",
                "jane.doe@example.com",
                UserType.Employee,
                null,
                null,
                "Engineering"),
            CancellationToken.None);

        Assert.Null(updated);
    }

    [Fact]
    public async Task UpdateAsync_WhenRequiredTypeSpecificFieldIsMissing_ThrowsUserValidationException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<UserValidationException>(() => service.UpdateAsync(
            new UpdateUserCommand(
                Guid.NewGuid(),
                "Jane",
                "Doe",
                "jane.doe@example.com",
                UserType.Employee,
                null,
                null,
                null),
            CancellationToken.None));
    }

    private static UserService CreateService()
    {
        return new UserService(new UserFactory(), new InMemoryUserRepository());
    }
}

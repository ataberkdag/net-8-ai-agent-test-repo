using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
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
    public async Task UpdateAsync_ReturnsUpdatedUser_WhenUserExists()
    {
        var userRepository = new Mock<IUserRepository>();
        var userFactory = new Mock<IUserFactory>();
        var existingUser = new AdminUser(
            Guid.NewGuid(),
            "Old",
            "Name",
            "old@example.com",
            "Admin",
            DateTime.UtcNow,
            "Level1");

        userRepository.Setup(repository => repository.GetByIdAsync(existingUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);
        userRepository.Setup(repository => repository.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) => user);

        var service = new UserService(userFactory.Object, userRepository.Object);
        var result = await service.UpdateAsync(
            new UpdateUserCommand(
                existingUser.Id,
                "New",
                "Name",
                "new@example.com",
                UserType.Admin,
                "Level2",
                null,
                null),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New", result.FirstName);
        Assert.Equal("Level2", result.PermissionLevel);
        userRepository.Verify(repository => repository.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        var userRepository = new Mock<IUserRepository>();
        var userFactory = new Mock<IUserFactory>();

        userRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var service = new UserService(userFactory.Object, userRepository.Object);
        var result = await service.UpdateAsync(
            new UpdateUserCommand(
                Guid.NewGuid(),
                "New",
                "Name",
                "new@example.com",
                UserType.Customer,
                null,
                "LC-1",
                null),
            CancellationToken.None);

        Assert.Null(result);
        userRepository.Verify(repository => repository.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsValidationException_WhenRequiredFieldMissing()
    {
        var userRepository = new Mock<IUserRepository>();
        var userFactory = new Mock<IUserFactory>();
        var service = new UserService(userFactory.Object, userRepository.Object);

        await Assert.ThrowsAsync<UserValidationException>(() =>
            service.UpdateAsync(
                new UpdateUserCommand(
                    Guid.NewGuid(),
                    string.Empty,
                    "Name",
                    "new@example.com",
                    UserType.Employee,
                    null,
                    null,
                    "Engineering"),
                CancellationToken.None));
    }
}
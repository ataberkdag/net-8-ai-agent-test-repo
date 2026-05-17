using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserManagement.Api.Contracts;
using UserManagement.Api.Controllers;
using UserManagement.Application.Abstractions;
using UserManagement.Application.Models;
using UserManagement.Domain.Enums;
using Xunit;

namespace UserManagement.Api.Tests;

public sealed class UsersControllerTests
{
    [Fact]
    public async Task Update_ReturnsOk_WhenUserIsUpdated()
    {
        var service = new Mock<IUserService>();
        var controller = new UsersController(service.Object);
        var id = Guid.NewGuid();

        service.Setup(s => s.UpdateAsync(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserResponse(
                id,
                "Jane",
                "Doe",
                "jane@example.com",
                UserType.Customer,
                DateTime.UtcNow,
                null,
                "LC-1",
                null));

        var result = await controller.Update(
            id,
            new UpdateUserRequest
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@example.com",
                UserType = UserType.Customer,
                LoyaltyCode = "LC-1"
            },
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<UserResponse>(okResult.Value);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var service = new Mock<IUserService>();
        var controller = new UsersController(service.Object);

        service.Setup(s => s.UpdateAsync(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserResponse?)null);

        var result = await controller.Update(
            Guid.NewGuid(),
            new UpdateUserRequest
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@example.com",
                UserType = UserType.Customer,
                LoyaltyCode = "LC-1"
            },
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }
}
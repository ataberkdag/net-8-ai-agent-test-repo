using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Controllers;
using UserManagement.Api.Contracts;
using UserManagement.Application.Abstractions;
using UserManagement.Application.Exceptions;
using UserManagement.Application.Models;
using UserManagement.Domain.Enums;
using Xunit;

namespace UserManagement.Api.Tests;

public sealed class UsersControllerTests
{
    [Fact]
    public async Task Update_ReturnsOk_WhenUserIsUpdated()
    {
        var service = new TestUserService
        {
            UpdateResult = new UserResponse(
                Guid.NewGuid(),
                "Updated",
                "User",
                "updated@example.com",
                UserType.Admin,
                DateTime.UtcNow,
                "L2",
                null,
                null)
        };

        var controller = new UsersController(service);
        var id = Guid.NewGuid();

        var result = await controller.Update(id, new UpdateUserRequest
        {
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@example.com",
            UserType = UserType.Admin,
            PermissionLevel = "L2"
        }, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<UserResponse>(okResult.Value);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var service = new TestUserService { UpdateResult = null };
        var controller = new UsersController(service);

        var result = await controller.Update(Guid.NewGuid(), new UpdateUserRequest
        {
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@example.com",
            UserType = UserType.Admin,
            PermissionLevel = "L2"
        }, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsValidationProblem_WhenValidationFails()
    {
        var service = new TestUserService { ThrowOnUpdate = true };
        var controller = new UsersController(service);

        var result = await controller.Update(Guid.NewGuid(), new UpdateUserRequest
        {
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@example.com",
            UserType = UserType.Admin,
            PermissionLevel = "L2"
        }, CancellationToken.None);

        Assert.IsType<ObjectResult>(result);
    }

    private sealed class TestUserService : IUserService
    {
        public UserResponse? UpdateResult { get; set; }
        public bool ThrowOnUpdate { get; set; }

        public Task<UserResponse> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task<UserResponse?> UpdateAsync(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            if (ThrowOnUpdate)
            {
                throw new UserValidationException("Validation failed.");
            }

            return Task.FromResult(UpdateResult);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Contracts;
using UserManagement.Api.Controllers;
using UserManagement.Application.Abstractions;
using UserManagement.Application.Models;
using UserManagement.Domain.Enums;
using Xunit;

namespace UserManagement.Tests;

public sealed class UsersControllerTests
{
    [Fact]
    public void Update_HasPutEndpointWithoutAdditionalRouteTemplate()
    {
        var controllerRoute = typeof(UsersController).GetCustomAttribute<RouteAttribute>();
        var method = typeof(UsersController).GetMethod(nameof(UsersController.Update));

        Assert.NotNull(controllerRoute);
        Assert.NotNull(method);

        var httpPut = Assert.Single(method!.GetCustomAttributes<HttpPutAttribute>());

        Assert.Equal("api/[controller]", controllerRoute!.Template);
        Assert.Null(httpPut.Template);
    }

    [Fact]
    public async Task Update_WhenServiceReturnsUser_ReturnsOkWithUpdatedUser()
    {
        var id = Guid.NewGuid();
        var updatedUser = new UserResponse(
            id,
            "Jane",
            "Doe",
            "jane.doe@example.com",
            UserType.Employee,
            DateTime.UtcNow,
            null,
            null,
            "Engineering");
        var service = new CapturingUserService
        {
            UpdateResult = updatedUser
        };
        var controller = new UsersController(service);
        var request = new UpdateUserRequest
        {
            Id = id,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@example.com",
            UserType = UserType.Employee,
            Department = "Engineering"
        };

        var result = await controller.Update(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(updatedUser, okResult.Value);
        Assert.Equal(
            new UpdateUserCommand(
                id,
                "Jane",
                "Doe",
                "jane.doe@example.com",
                UserType.Employee,
                null,
                null,
                "Engineering"),
            service.CapturedUpdateCommand);
    }

    [Fact]
    public async Task Update_WhenServiceReturnsNull_ReturnsNotFound()
    {
        var service = new CapturingUserService();
        var controller = new UsersController(service);
        var request = new UpdateUserRequest
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@example.com",
            UserType = UserType.Customer,
            LoyaltyCode = "LOYAL"
        };

        var result = await controller.Update(request, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    private sealed class CapturingUserService : IUserService
    {
        public UpdateUserCommand? CapturedUpdateCommand { get; private set; }

        public UserResponse? UpdateResult { get; init; }

        public Task<UserResponse> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserResponse?> UpdateAsync(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            CapturedUpdateCommand = command;

            return Task.FromResult(UpdateResult);
        }
    }
}

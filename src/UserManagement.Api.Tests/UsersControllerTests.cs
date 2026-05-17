using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Contracts;
using UserManagement.Api.Controllers;
using UserManagement.Application.Abstractions;
using UserManagement.Application.Exceptions;
using UserManagement.Application.Models;
using UserManagement.Domain.Enums;
using Xunit;

namespace UserManagement.Api.Tests;

public sealed class UsersControllerTests
{
    [Fact]
    public async Task Update_returns_ok_when_user_is_updated()
    {
        var expected = new UserResponse(
            Guid.NewGuid(),
            "Jane",
            "Doe",
            "jane@example.com",
            UserType.Customer,
            DateTime.UtcNow,
            null,
            "LOY-1",
            null);

        var controller = new UsersController(new TestUserService(expected));

        var result = await controller.Update(
            expected.Id,
            new UpdateUserRequest
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@example.com",
                UserType = UserType.Customer,
                LoyaltyCode = "LOY-1"
            },
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, okResult.Value);
    }

    [Fact]
    public async Task Update_returns_not_found_when_user_does_not_exist()
    {
        var controller = new UsersController(new TestUserService(null));

        var result = await controller.Update(
            Guid.NewGuid(),
            new UpdateUserRequest
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@example.com",
                UserType = UserType.Customer,
                LoyaltyCode = "LOY-1"
            },
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_returns_validation_problem_when_service_throws_validation_exception()
    {
        var controller = new UsersController(new ThrowingUserService());

        var result = await controller.Update(
            Guid.NewGuid(),
            new UpdateUserRequest
            {
                FirstName = "",
                LastName = "Doe",
                Email = "jane@example.com",
                UserType = UserType.Customer,
                LoyaltyCode = "LOY-1"
            },
            CancellationToken.None);

        Assert.IsType<ObjectResult>(result);
    }

    private sealed class TestUserService : IUserService
    {
        private readonly UserResponse? _response;

        public TestUserService(UserResponse? response)
        {
            _response = response;
        }

        public Task<UserResponse> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserResponse?> UpdateAsync(Guid id, CreateUserCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }

        public Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class ThrowingUserService : IUserService
    {
        public Task<UserResponse> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserResponse?> UpdateAsync(Guid id, CreateUserCommand command, CancellationToken cancellationToken)
        {
            throw new UserValidationException("FirstName is required.");
        }

        public Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

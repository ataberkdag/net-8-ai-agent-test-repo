using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Contracts;
using UserManagement.Application.Abstractions;
using UserManagement.Application.Exceptions;
using UserManagement.Application.Models;

namespace UserManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateUserCommand(
                request.FirstName,
                request.LastName,
                request.Email,
                request.UserType,
                request.PermissionLevel,
                request.LoyaltyCode,
                request.Department);

            var createdUser = await _userService.CreateAsync(command, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }
        catch (UserValidationException exception)
        {
            return ValidationProblem(detail: exception.Message);
        }
    }

    [HttpPut]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> Update([FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateUserCommand(
                request.Id,
                request.FirstName,
                request.LastName,
                request.Email,
                request.UserType,
                request.PermissionLevel,
                request.LoyaltyCode,
                request.Department);

            var updatedUser = await _userService.UpdateAsync(command, cancellationToken);

            if (updatedUser is null)
            {
                return NotFound();
            }

            return Ok(updatedUser);
        }
        catch (UserValidationException exception)
        {
            return ValidationProblem(detail: exception.Message);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<UserResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);

        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}

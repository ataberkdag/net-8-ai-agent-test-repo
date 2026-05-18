using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Application.Abstractions;
using UserManagement.Application.Exceptions;
using UserManagement.Application.Models;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;

namespace UserManagement.Application.Services;

public sealed class UserService : IUserService
{
    private readonly IUserFactory _userFactory;
    private readonly IUserRepository _userRepository;

    public UserService(IUserFactory userFactory, IUserRepository userRepository)
    {
        _userFactory = userFactory;
        _userRepository = userRepository;
    }

    public async Task<UserResponse> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        Validate(command);

        var user = _userFactory.Create(command);
        var createdUser = await _userRepository.AddAsync(user, cancellationToken);

        return MapToResponse(createdUser);
    }

    public async Task<UserResponse?> UpdateAsync(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        Validate(command);

        var existingUser = await _userRepository.GetByIdAsync(command.Id, cancellationToken);

        if (existingUser is null)
        {
            return null;
        }

        var updatedUser = CreateUpdatedUser(command, existingUser.CreatedAtUtc);
        var savedUser = await _userRepository.UpdateAsync(updatedUser, cancellationToken);

        return savedUser is null ? null : MapToResponse(savedUser);
    }

    public async Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        return users
            .Select(MapToResponse)
            .ToArray();
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        return user is null ? null : MapToResponse(user);
    }

    private static void Validate(CreateUserCommand command)
    {
        Validate(
            command.FirstName,
            command.LastName,
            command.Email,
            command.UserType,
            command.PermissionLevel,
            command.LoyaltyCode,
            command.Department);
    }

    private static void Validate(UpdateUserCommand command)
    {
        if (command.Id == Guid.Empty)
        {
            throw new UserValidationException("Id is required.");
        }

        Validate(
            command.FirstName,
            command.LastName,
            command.Email,
            command.UserType,
            command.PermissionLevel,
            command.LoyaltyCode,
            command.Department);
    }

    private static void Validate(
        string firstName,
        string lastName,
        string email,
        UserType userType,
        string? permissionLevel,
        string? loyaltyCode,
        string? department)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new UserValidationException("FirstName is required.");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new UserValidationException("LastName is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new UserValidationException("Email is required.");
        }

        if (!IsValidEmail(email))
        {
            throw new UserValidationException("Email format is invalid.");
        }

        switch (userType)
        {
            case UserType.Admin when string.IsNullOrWhiteSpace(permissionLevel):
                throw new UserValidationException("PermissionLevel is required for admin users.");
            case UserType.Customer when string.IsNullOrWhiteSpace(loyaltyCode):
                throw new UserValidationException("LoyaltyCode is required for customer users.");
            case UserType.Employee when string.IsNullOrWhiteSpace(department):
                throw new UserValidationException("Department is required for employee users.");
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static User CreateUpdatedUser(UpdateUserCommand command, DateTime createdAtUtc)
    {
        return command.UserType switch
        {
            UserType.Admin => new AdminUser(
                command.Id,
                command.FirstName,
                command.LastName,
                command.Email,
                command.PermissionLevel!,
                createdAtUtc),
            UserType.Customer => new CustomerUser(
                command.Id,
                command.FirstName,
                command.LastName,
                command.Email,
                command.LoyaltyCode!,
                createdAtUtc),
            UserType.Employee => new EmployeeUser(
                command.Id,
                command.FirstName,
                command.LastName,
                command.Email,
                command.Department!,
                createdAtUtc),
            _ => throw new InvalidOperationException("Unsupported user type.")
        };
    }

    private static UserResponse MapToResponse(User user)
    {
        return user switch
        {
            AdminUser admin => new UserResponse(
                admin.Id,
                admin.FirstName,
                admin.LastName,
                admin.Email,
                admin.UserType,
                admin.CreatedAtUtc,
                admin.PermissionLevel,
                null,
                null),
            CustomerUser customer => new UserResponse(
                customer.Id,
                customer.FirstName,
                customer.LastName,
                customer.Email,
                customer.UserType,
                customer.CreatedAtUtc,
                null,
                customer.LoyaltyCode,
                null),
            EmployeeUser employee => new UserResponse(
                employee.Id,
                employee.FirstName,
                employee.LastName,
                employee.Email,
                employee.UserType,
                employee.CreatedAtUtc,
                null,
                null,
                employee.Department),
            _ => throw new InvalidOperationException("Unsupported user type.")
        };
    }
}

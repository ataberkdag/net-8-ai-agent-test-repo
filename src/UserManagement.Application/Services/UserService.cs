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
        Validate(command.FirstName, command.LastName, command.Email, command.UserType, command.PermissionLevel, command.LoyaltyCode, command.Department);

        var user = _userFactory.Create(command);
        var createdUser = await _userRepository.AddAsync(user, cancellationToken);

        return MapToResponse(createdUser);
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

    public async Task<UserResponse?> UpdateAsync(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        Validate(command.FirstName, command.LastName, command.Email, command.UserType, command.PermissionLevel, command.LoyaltyCode, command.Department);

        var existingUser = await _userRepository.GetByIdAsync(command.Id, cancellationToken);

        if (existingUser is null)
        {
            return null;
        }

        var updatedUser = _userFactory.Create(command, existingUser.CreatedAtUtc);
        var savedUser = await _userRepository.UpdateAsync(updatedUser, cancellationToken);

        return savedUser is null ? null : MapToResponse(savedUser);
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
            _ = new System.Net.Mail.MailAddress(email);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
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

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

    public async Task<UserResponse?> UpdateAsync(Guid id, CreateUserCommand command, CancellationToken cancellationToken)
    {
        Validate(command);

        var existingUser = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (existingUser is null)
        {
            return null;
        }

        var updatedUser = _userFactory.Create(command) with { Id = id, CreatedAtUtc = existingUser.CreatedAtUtc };
        var persistedUser = await _userRepository.UpdateAsync(updatedUser, cancellationToken);

        return MapToResponse(persistedUser);
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
        if (string.IsNullOrWhiteSpace(command.FirstName))
        {
            throw new UserValidationException("FirstName is required.");
        }

        if (string.IsNullOrWhiteSpace(command.LastName))
        {
            throw new UserValidationException("LastName is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new UserValidationException("Email is required.");
        }

        if (!IsValidEmail(command.Email))
        {
            throw new UserValidationException("Email format is invalid.");
        }

        switch (command.UserType)
        {
            case UserType.Admin when string.IsNullOrWhiteSpace(command.PermissionLevel):
                throw new UserValidationException("PermissionLevel is required for admin users.");
            case UserType.Customer when string.IsNullOrWhiteSpace(command.LoyaltyCode):
                throw new UserValidationException("LoyaltyCode is required for customer users.");
            case UserType.Employee when string.IsNullOrWhiteSpace(command.Department):
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

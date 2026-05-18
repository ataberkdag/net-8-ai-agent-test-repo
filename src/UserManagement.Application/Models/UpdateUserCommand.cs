using UserManagement.Domain.Enums;

namespace UserManagement.Application.Models;

public sealed record UpdateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    UserType UserType,
    string? PermissionLevel,
    string? LoyaltyCode,
    string? Department);

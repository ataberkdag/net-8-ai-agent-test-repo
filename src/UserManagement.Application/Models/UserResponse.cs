using UserManagement.Domain.Enums;

namespace UserManagement.Application.Models;

public sealed record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserType UserType,
    DateTime CreatedAtUtc,
    string? PermissionLevel,
    string? LoyaltyCode,
    string? Department);

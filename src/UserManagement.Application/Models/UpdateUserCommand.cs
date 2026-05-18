using System;
using UserManagement.Domain.Enums;

namespace UserManagement.Application.Models;

public sealed record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserType UserType,
    string? PermissionLevel,
    string? LoyaltyCode,
    string? Department);

using System.ComponentModel.DataAnnotations;
using UserManagement.Domain.Enums;

namespace UserManagement.Api.Contracts;

public sealed class CreateUserRequest
{
    [Required]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    public string LastName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public UserType UserType { get; init; }

    public string? PermissionLevel { get; init; }

    public string? LoyaltyCode { get; init; }

    public string? Department { get; init; }
}

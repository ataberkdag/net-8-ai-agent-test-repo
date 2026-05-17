using UserManagement.Domain.Enums;

namespace UserManagement.Domain.Entities;

public sealed class CustomerUser : User
{
    public CustomerUser(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string loyaltyCode,
        DateTime createdAtUtc)
        : base(id, firstName, lastName, email, UserType.Customer, createdAtUtc)
    {
        LoyaltyCode = loyaltyCode;
    }

    public string LoyaltyCode { get; }
}

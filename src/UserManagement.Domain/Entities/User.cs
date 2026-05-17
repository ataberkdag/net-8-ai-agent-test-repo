using UserManagement.Domain.Enums;

namespace UserManagement.Domain.Entities;

public abstract class User
{
    protected User(
        Guid id,
        string firstName,
        string lastName,
        string email,
        UserType userType,
        DateTime createdAtUtc)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        UserType = userType;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; }

    public string FirstName { get; }

    public string LastName { get; }

    public string Email { get; }

    public UserType UserType { get; }

    public DateTime CreatedAtUtc { get; }
}

using UserManagement.Domain.Enums;

namespace UserManagement.Domain.Entities;

public sealed class EmployeeUser : User
{
    public EmployeeUser(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string department,
        DateTime createdAtUtc)
        : base(id, firstName, lastName, email, UserType.Employee, createdAtUtc)
    {
        Department = department;
    }

    public string Department { get; }
}

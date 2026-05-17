namespace UserManagement.Application.Exceptions;

public sealed class UserValidationException : Exception
{
    public UserValidationException(string message)
        : base(message)
    {
    }
}

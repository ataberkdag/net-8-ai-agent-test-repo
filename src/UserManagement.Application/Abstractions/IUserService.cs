using UserManagement.Application.Models;

namespace UserManagement.Application.Abstractions;

public interface IUserService
{
    Task<UserResponse> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<UserResponse?> UpdateAsync(UpdateUserCommand command, CancellationToken cancellationToken);
}

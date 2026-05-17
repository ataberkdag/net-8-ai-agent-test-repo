using UserManagement.Domain.Entities;

namespace UserManagement.Application.Abstractions;

public interface IUserRepository
{
    Task<User> AddAsync(User user, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

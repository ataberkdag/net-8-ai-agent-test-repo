using System.Collections.Concurrent;
using UserManagement.Application.Abstractions;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Repositories;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();

    public Task<User> AddAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _users[user.Id] = user;

        return Task.FromResult(user);
    }

    public Task<User> UpdateAsync(Guid id, User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_users.ContainsKey(id))
        {
            throw new KeyNotFoundException($"User with id '{id}' was not found.");
        }

        _users[id] = user;

        return Task.FromResult(user);
    }

    public Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyCollection<User> users = _users.Values
            .OrderBy(user => user.CreatedAtUtc)
            .ToArray();

        return Task.FromResult(users);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _users.TryGetValue(id, out var user);

        return Task.FromResult(user);
    }
}

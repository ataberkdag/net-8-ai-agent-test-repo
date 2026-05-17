using Microsoft.Extensions.DependencyInjection;
using UserManagement.Application.Abstractions;
using UserManagement.Application.Services;
using UserManagement.Infrastructure.Factories;
using UserManagement.Infrastructure.Repositories;

namespace UserManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        services.AddSingleton<IUserFactory, UserFactory>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}

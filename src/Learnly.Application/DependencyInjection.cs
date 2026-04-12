using Microsoft.Extensions.DependencyInjection;

namespace Learnly.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application services, validators, and mapping registration will be added in later steps.
        return services;
    }
}

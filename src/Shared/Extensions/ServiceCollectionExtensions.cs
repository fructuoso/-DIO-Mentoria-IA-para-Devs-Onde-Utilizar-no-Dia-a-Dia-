using Microsoft.Extensions.DependencyInjection;
using Shared.Messaging;
using Shared.Services;

namespace Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services, string rabbitMQConnectionString = "amqp://localhost")
    {
        // Messaging
        services.AddSingleton<IMessagePublisher>(provider =>
            new RabbitMQService(provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RabbitMQService>>(), rabbitMQConnectionString));
        
        services.AddSingleton<IMessageConsumer>(provider =>
            provider.GetRequiredService<IMessagePublisher>() as RabbitMQService ?? 
            throw new InvalidOperationException("RabbitMQService not registered"));

        // JWT Service
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }
}

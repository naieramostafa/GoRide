using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RideSharing.Application.Interfaces;
using RideSharing.Infrastructure.BackgroundJobs;
using RideSharing.Infrastructure.Messaging;
using RideSharing.Infrastructure.Persistence;
using RideSharing.Infrastructure.Services.Authentication;
using RideSharing.Infrastructure.Services.Cache;
using RideSharing.Infrastructure.Services.Maps;
using RideSharing.Infrastructure.Services.Notifications;
using RideSharing.Infrastructure.Services.Payment;

namespace RideSharing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<RideSharingDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPaymentService, StripePaymentService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddHttpClient<IMapService, OsrmMapService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IMessageBus, RabbitMqBus>();
        services.AddHostedService<RideMatchingBackgroundService>();

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}

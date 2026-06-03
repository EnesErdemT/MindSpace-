using Blog.Application.Common.Interfaces;
using Blog.Application.Features.Notifications.Interfaces;
using Blog.Infrastructure.Data;
using Blog.Infrastructure.Repositories;
using Blog.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Blog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Context
        services.AddDbContext<BlogDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repository Pattern
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Token Blacklist (singleton - trăiește pe toată durata aplicației)
        services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();

        // Email
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddScoped<IEmailService, EmailService>();

        // Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationHubService, NotificationHubService>();
        
        // RabbitMQ Services
        services.AddScoped<IMessagePublisher, RabbitMqMessagePublisher>();
        
        // Elasticsearch Services
        services.AddSingleton<IElasticsearchService, ElasticsearchService>();
        
        // Background Services (Consumers)
        services.AddHostedService<Services.Consumers.NotificationConsumer>();
        services.AddHostedService<Services.Consumers.EmailConsumer>();

        return services;
    }
} 
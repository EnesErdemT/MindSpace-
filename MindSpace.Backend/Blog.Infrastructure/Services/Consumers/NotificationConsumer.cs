using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Messages;
using Blog.Application.Features.Notifications.Interfaces;
using Blog.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Blog.Infrastructure.Services.Consumers;

public class NotificationConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationConsumer> _logger;

    public NotificationConsumer(
        IServiceProvider serviceProvider, 
        ILogger<NotificationConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🏯 NotificationConsumer pornit");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(5000, stoppingToken);
                
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("📡 NotificationConsumer ascultă mesaje...");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("🛑 NotificationConsumer se oprește...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Eroare în NotificationConsumer");
                await Task.Delay(1000, stoppingToken); 
            }
        }
    }

    public async Task HandlePostLikedAsync(PostLikedMessage message)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var hubService = scope.ServiceProvider.GetRequiredService<INotificationHubService>();

            _logger.LogInformation("📨 Procesare PostLikedMessage: {PostId} apreciat de {LikerUserId}", 
                message.PostId, message.LikerUserId);

            await notificationService.CreateNotificationAsync(
                userId: message.PostAuthorId,
                title: "Postare apreciată",
                message: $"{message.LikerUserName} a apreciat postarea dvs.: {message.PostTitle}",
                type: NotificationType.PostLiked,
                actionUrl: $"/post/{message.PostId}",
                actorId: message.LikerUserId,
                postId: message.PostId
            );

            _logger.LogInformation("✅ PostLikedMessage procesat cu succes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Eșec la procesarea PostLikedMessage: {MessageId}", message.Id);
            throw;
        }
    }

    public async Task HandleNewCommentAsync(NewCommentMessage message)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            _logger.LogInformation("📨 Procesare NewCommentMessage: Comentariu {CommentId} la postarea {PostId}", 
                message.CommentId, message.PostId);

            await notificationService.CreateNotificationAsync(
                userId: message.PostAuthorId,
                title: "Comentariu nou",
                message: $"{message.CommenterUserName} a comentat la postarea dvs.: {message.PostTitle}",
                type: NotificationType.NewComment,
                actionUrl: $"/post/{message.PostId}",
                actorId: message.CommenterUserId,
                postId: message.PostId,
                commentId: message.CommentId
            );

            _logger.LogInformation("✅ NewCommentMessage procesat cu succes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Eșec la procesarea NewCommentMessage: {MessageId}", message.Id);
            throw;
        }
    }

    public async Task HandlePostPublishedAsync(PostPublishedMessage message)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            _logger.LogInformation("📨 Procesare PostPublishedMessage: Postare {PostId} de {AuthorName}", 
                message.PostId, message.AuthorName);

            foreach (var followerId in message.FollowerIds)
            {
                await notificationService.CreateNotificationAsync(
                    userId: followerId,
                    title: "Postare nouă",
                    message: $"{message.AuthorName} a publicat o postare nouă: {message.PostTitle}",
                    type: NotificationType.PostPublished,
                    actionUrl: $"/post/{message.PostSlug}",
                    actorId: message.AuthorId,
                    postId: message.PostId
                );
            }

            _logger.LogInformation("✅ PostPublishedMessage procesat cu succes pentru {FollowerCount} urmăritori", 
                message.FollowerIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Eșec la procesarea PostPublishedMessage: {MessageId}", message.Id);
            throw;
        }
    }
} 

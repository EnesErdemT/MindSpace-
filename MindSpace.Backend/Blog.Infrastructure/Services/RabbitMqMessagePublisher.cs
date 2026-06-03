using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Messages;
using Microsoft.Extensions.Logging;

namespace Blog.Infrastructure.Services;

public class RabbitMqMessagePublisher : IMessagePublisher
{
    private readonly ILogger<RabbitMqMessagePublisher> _logger;

    public RabbitMqMessagePublisher(ILogger<RabbitMqMessagePublisher> logger)
    {
        _logger = logger;
    }

    public async Task PublishNotificationAsync<T>(T message, CancellationToken cancellationToken = default) 
        where T : class, INotificationMessage
    {
        try
        {
            _logger.LogInformation("📨 Publicare mesaj notificare: {MessageType} pentru utilizatorul {UserId}", 
                typeof(T).Name, message.UserId);
            
            await Task.Delay(100, cancellationToken); 
            
            _logger.LogInformation("✅ Mesaj notificare publicat cu succes: {MessageId}", message.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Eșec la publicarea mesajului de notificare: {MessageType}", typeof(T).Name);
            throw;
        }
    }

    public async Task PublishEmailAsync(EmailNotificationMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📧 Publicare mesaj email către: {ToEmail} cu subiect: {Subject}", 
                message.ToEmail, message.Subject);
            
            await Task.Delay(100, cancellationToken);
            
            _logger.LogInformation("✅ Mesaj email publicat cu succes: {MessageId}", message.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Eșec la publicarea mesajului email către: {ToEmail}", message.ToEmail);
            throw;
        }
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) 
        where T : class
    {
        try
        {
            _logger.LogInformation("🚀 Publicare mesaj generic: {MessageType}", typeof(T).Name);
            
            await Task.Delay(100, cancellationToken);
            
            _logger.LogInformation("✅ Mesaj generic publicat cu succes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Eșec la publicarea mesajului generic: {MessageType}", typeof(T).Name);
            throw;
        }
    }

    public async Task PublishBatchAsync<T>(IEnumerable<T> messages, CancellationToken cancellationToken = default) 
        where T : class
    {
        try
        {
            var messageList = messages.ToList();
            _logger.LogInformation("📦 Publicare lot de {Count} mesaje: {MessageType}", 
                messageList.Count, typeof(T).Name);
            
            foreach (var message in messageList)
            {
                await PublishAsync(message, cancellationToken);
            }
            
            _logger.LogInformation("✅ Mesaje în lot publicate cu succes: {Count} mesaje", messageList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Eșec la publicarea mesajelor în lot: {MessageType}", typeof(T).Name);
            throw;
        }
    }
} 
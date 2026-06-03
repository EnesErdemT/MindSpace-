using Blog.Domain.Common;

namespace Blog.Domain.Entities;

public class Notification : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    
    public string? ActionUrl { get; set; } // URL unde se va naviga la click pe notificare
    public string? ActionData { get; set; } // Date suplimentare în format JSON
    public string UserId { get; set; } = string.Empty; // Utilizatorul care va primi notificarea
    public string? ActorId { get; set; } // Utilizatorul care a declanșat notificarea
    public Guid? PostId { get; set; }
    public Guid? CommentId { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual User? Actor { get; set; }
    public virtual Post? Post { get; set; }
    public virtual Comment? Comment { get; set; }
}

/// <summary>
/// Enum pentru tipul de notificare
/// </summary>
public enum NotificationType
{
    NewComment = 0,     // Comentariu nou la postare
    NewLike = 1,        // Postarea a fost apreciată
    PostLiked = 2,      // Postare apreciată (alias pentru NewLike)
    NewFollower = 3,    // Urmăritor nou
    CommentLike = 4,    // Comentariul a fost apreciat
    CommentReply = 5,   // Răspuns la comentariu
    PostPublished = 6   // Persoana urmărită a publicat o postare
} 
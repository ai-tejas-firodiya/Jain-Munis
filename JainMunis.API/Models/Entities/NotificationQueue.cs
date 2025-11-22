using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JainMunis.API.Models.Entities;

public class NotificationQueue
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(255)]
    public string UserIdentifier { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Channel { get; set; } = string.Empty; // 'email', 'push', 'whatsapp'

    [Required]
    public string MessageContent { get; set; } = string.Empty; // JSON string

    public DateTime? ScheduledAt { get; set; }

    public DateTime? SentAt { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "pending"; // 'pending', 'sent', 'failed'

    public int RetryCount { get; set; } = 0;

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid NotificationSubscriptionId { get; set; }

    [ForeignKey("NotificationSubscriptionId")]
    public virtual NotificationSubscription NotificationSubscription { get; set; } = null!;
}
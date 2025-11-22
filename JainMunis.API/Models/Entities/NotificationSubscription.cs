using System.ComponentModel.DataAnnotations;

namespace JainMunis.API.Models.Entities;

public class NotificationSubscription
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(255)]
    public string UserIdentifier { get; set; } = string.Empty; // email or phone

    public string? PushEndpoint { get; set; } // browser push subscription JSON

    public bool EmailEnabled { get; set; } = true;

    public bool WhatsAppEnabled { get; set; } = false;

    public bool PushEnabled { get; set; } = false;

    public string? PreferredCities { get; set; } // JSON array of cities

    public string? FollowedSaints { get; set; } // JSON array of saint IDs

    public string? NotificationTypes { get; set; } // JSON string for detailed preferences

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<NotificationQueue> QueuedNotifications { get; set; } = new List<NotificationQueue>();
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JainMunis.API.Models.Entities;

public class ActivityLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? AdminUserId { get; set; }

    [ForeignKey("AdminUserId")]
    public virtual AdminUser? AdminUser { get; set; }

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty; // e.g., 'CREATE_SCHEDULE', 'UPDATE_SAINT'

    [MaxLength(50)]
    public string? EntityType { get; set; } // 'saint', 'schedule', 'location'

    public Guid? EntityId { get; set; }

    public string? OldValues { get; set; } // JSON string

    public string? NewValues { get; set; } // JSON string

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
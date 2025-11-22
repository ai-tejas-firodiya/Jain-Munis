using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace JainMunis.API.Models.Entities;

public class AdminUser : IdentityUser
{
    [MaxLength(50)]
    public string Role { get; set; } = "admin"; // admin, super_admin, view_only

    public string? Permissions { get; set; } // JSON string for granular permissions

    public bool IsActive { get; set; } = true;

    public DateTime? LastLogin { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
}
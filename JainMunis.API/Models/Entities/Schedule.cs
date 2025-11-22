using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JainMunis.API.Models.Entities;

public class Schedule
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SaintId { get; set; }

    [ForeignKey("SaintId")]
    public virtual Saint Saint { get; set; } = null!;

    [Required]
    public Guid LocationId { get; set; }

    [ForeignKey("LocationId")]
    public virtual Location Location { get; set; } = null!;

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    public string? Purpose { get; set; } // e.g., "Pravachan", "Chaturmas", "Visit"

    public string? Notes { get; set; } // additional information for devotees

    [MaxLength(255)]
    public string? ContactPerson { get; set; } // local contact for devotees

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    public Guid? CreatedBy { get; set; } // admin user who created this entry

    [ForeignKey("CreatedBy")]
    public virtual AdminUser? Creator { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
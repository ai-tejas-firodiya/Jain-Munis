using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JainMunis.API.Models.Entities;

public class Location
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty; // e.g., "Jain Temple, Mumbai"

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string Country { get; set; } = "India";

    [Column(TypeName = "decimal(10, 8)")]
    public decimal? Latitude { get; set; } // for geospatial queries

    [Column(TypeName = "decimal(11, 8)")]
    public decimal? Longitude { get; set; } // for geospatial queries

    // SQL Server spatial data type
    public string? LocationGeography { get; set; }

    [MaxLength(20)]
    public string? ContactPhone { get; set; } // optional

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
namespace JainMunis.API.Models.DTOs;

public class SaintDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? SpiritualLineage { get; set; }
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ScheduleDto? CurrentSchedule { get; set; }
    public List<ScheduleDto> UpcomingSchedules { get; set; } = new();
}

public class CreateSaintRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? SpiritualLineage { get; set; }
    public string? Bio { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class UpdateSaintRequest
{
    public string? Name { get; set; }
    public string? Title { get; set; }
    public string? SpiritualLineage { get; set; }
    public string? Bio { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
}
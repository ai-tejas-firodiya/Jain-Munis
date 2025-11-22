namespace JainMunis.API.Models.DTOs;

public class ScheduleDto
{
    public Guid Id { get; set; }
    public Guid SaintId { get; set; }
    public SaintDto? Saint { get; set; }
    public Guid LocationId { get; set; }
    public LocationDto? Location { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Purpose { get; set; }
    public string? Notes { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsUpcoming { get; set; }
}

public class CreateScheduleRequest
{
    public Guid SaintId { get; set; }
    public Guid LocationId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Purpose { get; set; }
    public string? Notes { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
}

public class UpdateScheduleRequest
{
    public Guid? SaintId { get; set; }
    public Guid? LocationId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Purpose { get; set; }
    public string? Notes { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
}
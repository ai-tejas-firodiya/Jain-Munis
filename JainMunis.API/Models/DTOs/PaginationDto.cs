namespace JainMunis.API.Models.DTOs;

public class PaginationDto
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
    public int Total { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / Limit);
}

public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public T? Data { get; set; }
    public PaginationDto? Pagination { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public ErrorDetail Error { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ErrorDetail
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }
}

public class SearchParams
{
    public string? Search { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int? RadiusKm { get; set; }
    public Guid? SaintId { get; set; }
    public int? DaysAhead { get; set; }
}
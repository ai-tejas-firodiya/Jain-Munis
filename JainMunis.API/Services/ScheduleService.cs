using Microsoft.EntityFrameworkCore;
using JainMunis.API.Data;
using JainMunis.API.Models.DTOs;
using JainMunis.API.Models.Entities;

namespace JainMunis.API.Services;

public class ScheduleService : IScheduleService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthService _authService;

    public ScheduleService(ApplicationDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<(List<ScheduleDto> schedules, int total)> GetSchedulesAsync(int page, int limit, SearchParams? searchParams = null)
    {
        var query = _context.Schedules
            .Include(sc => sc.Saint)
            .Include(sc => sc.Location)
            .AsQueryable();

        // Apply filters
        if (searchParams != null)
        {
            if (searchParams.SaintId.HasValue)
            {
                query = query.Where(sc => sc.SaintId == searchParams.SaintId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchParams.City))
            {
                query = query.Where(sc => sc.Location.City.ToLower() == searchParams.City.ToLower());
            }

            if (searchParams.DateFrom.HasValue)
            {
                var dateFrom = DateOnly.FromDateTime(searchParams.DateFrom.Value);
                query = query.Where(sc => sc.StartDate >= dateFrom);
            }

            if (searchParams.DateTo.HasValue)
            {
                var dateTo = DateOnly.FromDateTime(searchParams.DateTo.Value);
                query = query.Where(sc => sc.EndDate <= dateTo);
            }
        }

        // Get total count
        var total = await query.CountAsync();

        // Apply pagination
        var schedules = await query
            .OrderByDescending(sc => sc.StartDate)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return (schedules.Select(ConvertToDto).ToList(), total);
    }

    public async Task<ScheduleDto?> GetScheduleByIdAsync(Guid id)
    {
        var schedule = await _context.Schedules
            .Include(sc => sc.Saint)
            .Include(sc => sc.Location)
            .Include(sc => sc.Creator)
            .FirstOrDefaultAsync(sc => sc.Id == id);

        return schedule != null ? ConvertToDto(schedule) : null;
    }

    public async Task<ScheduleDto> CreateScheduleAsync(CreateScheduleRequest request, Guid? createdBy)
    {
        // Check for overlapping schedules
        var overlaps = await CheckOverlapsAsync(request.SaintId, request.StartDate, request.EndDate);
        if (overlaps.Any())
        {
            throw new InvalidOperationException("Schedule conflicts with existing schedules for this saint.");
        }

        var schedule = new Schedule
        {
            SaintId = request.SaintId,
            LocationId = request.LocationId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Purpose = request.Purpose,
            Notes = request.Notes,
            ContactPerson = request.ContactPerson,
            ContactPhone = request.ContactPhone,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();

        // Log activity
        await _authService.LogActivityAsync(
            createdBy,
            "CREATE_SCHEDULE",
            "schedule",
            schedule.Id,
            null,
            System.Text.Json.JsonSerializer.Serialize(new
            {
                schedule.SaintId,
                schedule.LocationId,
                schedule.StartDate,
                schedule.EndDate,
                schedule.Purpose
            }),
            null,
            null
        );

        return await GetScheduleByIdAsync(schedule.Id) ?? throw new InvalidOperationException("Failed to create schedule");
    }

    public async Task<ScheduleDto?> UpdateScheduleAsync(Guid id, UpdateScheduleRequest request, Guid? updatedBy)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule == null)
        {
            return null;
        }

        var oldValues = new
        {
            schedule.SaintId,
            schedule.LocationId,
            schedule.StartDate,
            schedule.EndDate,
            schedule.Purpose,
            schedule.Notes,
            schedule.ContactPerson,
            schedule.ContactPhone
        };

        // Check for overlapping schedules (excluding current schedule)
        var startDate = request.StartDate ?? schedule.StartDate;
        var endDate = request.EndDate ?? schedule.EndDate;
        var saintId = request.SaintId ?? schedule.SaintId;

        var overlaps = await CheckOverlapsAsync(saintId, startDate, endDate, id);
        if (overlaps.Any())
        {
            throw new InvalidOperationException("Schedule conflicts with existing schedules for this saint.");
        }

        if (request.SaintId.HasValue)
            schedule.SaintId = request.SaintId.Value;
        if (request.LocationId.HasValue)
            schedule.LocationId = request.LocationId.Value;
        if (request.StartDate.HasValue)
            schedule.StartDate = request.StartDate.Value;
        if (request.EndDate.HasValue)
            schedule.EndDate = request.EndDate.Value;
        if (request.Purpose != null)
            schedule.Purpose = request.Purpose;
        if (request.Notes != null)
            schedule.Notes = request.Notes;
        if (request.ContactPerson != null)
            schedule.ContactPerson = request.ContactPerson;
        if (request.ContactPhone != null)
            schedule.ContactPhone = request.ContactPhone;

        schedule.UpdatedAt = DateTime.UtcNow;

        var newValues = new
        {
            schedule.SaintId,
            schedule.LocationId,
            schedule.StartDate,
            schedule.EndDate,
            schedule.Purpose,
            schedule.Notes,
            schedule.ContactPerson,
            schedule.ContactPhone
        };

        await _context.SaveChangesAsync();

        // Log activity
        await _authService.LogActivityAsync(
            updatedBy,
            "UPDATE_SCHEDULE",
            "schedule",
            schedule.Id,
            System.Text.Json.JsonSerializer.Serialize(oldValues),
            System.Text.Json.JsonSerializer.Serialize(newValues),
            null,
            null
        );

        return await GetScheduleByIdAsync(schedule.Id);
    }

    public async Task<bool> DeleteScheduleAsync(Guid id)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule == null)
        {
            return false;
        }

        var oldValues = new
        {
            schedule.SaintId,
            schedule.LocationId,
            schedule.StartDate,
            schedule.EndDate,
            schedule.Purpose
        };

        _context.Schedules.Remove(schedule);
        await _context.SaveChangesAsync();

        // Log activity
        await _authService.LogActivityAsync(
            null, // TODO: Get current admin user ID
            "DELETE_SCHEDULE",
            "schedule",
            schedule.Id,
            System.Text.Json.JsonSerializer.Serialize(oldValues),
            null,
            null,
            null
        );

        return true;
    }

    public async Task<List<ScheduleDto>> GetCurrentSchedulesAsync(SearchParams? searchParams = null)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var query = _context.Schedules
            .Where(sc => sc.StartDate <= today && sc.EndDate >= today)
            .Include(sc => sc.Saint)
            .Include(sc => sc.Location)
            .AsQueryable();

        if (searchParams != null)
        {
            if (!string.IsNullOrWhiteSpace(searchParams.City))
            {
                query = query.Where(sc => sc.Location.City.ToLower() == searchParams.City.ToLower());
            }

            if (searchParams.SaintId.HasValue)
            {
                query = query.Where(sc => sc.SaintId == searchParams.SaintId.Value);
            }
        }

        var schedules = await query
            .OrderBy(sc => sc.EndDate)
            .ToListAsync();

        return schedules.Select(ConvertToDto).ToList();
    }

    public async Task<List<ScheduleDto>> GetUpcomingSchedulesAsync(SearchParams? searchParams = null)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysAhead = searchParams?.DaysAhead ?? 30;
        var endDate = today.AddDays(daysAhead);

        var query = _context.Schedules
            .Where(sc => sc.StartDate > today && sc.StartDate <= endDate)
            .Include(sc => sc.Saint)
            .Include(sc => sc.Location)
            .AsQueryable();

        if (searchParams != null)
        {
            if (!string.IsNullOrWhiteSpace(searchParams.City))
            {
                query = query.Where(sc => sc.Location.City.ToLower() == searchParams.City.ToLower());
            }

            if (searchParams.SaintId.HasValue)
            {
                query = query.Where(sc => sc.SaintId == searchParams.SaintId.Value);
            }
        }

        var schedules = await query
            .OrderBy(sc => sc.StartDate)
            .ToListAsync();

        return schedules.Select(ConvertToDto).ToList();
    }

    public async Task<List<ScheduleDto>> GetSchedulesBySaintAsync(Guid saintId)
    {
        var schedules = await _context.Schedules
            .Where(sc => sc.SaintId == saintId)
            .Include(sc => sc.Saint)
            .Include(sc => sc.Location)
            .OrderByDescending(sc => sc.StartDate)
            .ToListAsync();

        return schedules.Select(ConvertToDto).ToList();
    }

    public async Task<List<ScheduleDto>> CheckOverlapsAsync(Guid saintId, DateOnly startDate, DateOnly endDate, Guid? excludeScheduleId = null)
    {
        var query = _context.Schedules
            .Where(sc => sc.SaintId == saintId)
            .Where(sc =>
                (sc.StartDate <= startDate && sc.EndDate >= startDate) || // Schedule starts during existing schedule
                (sc.StartDate <= endDate && sc.EndDate >= endDate) ||     // Schedule ends during existing schedule
                (sc.StartDate >= startDate && sc.EndDate <= endDate)       // Schedule is completely within new schedule
            )
            .Include(sc => sc.Saint)
            .Include(sc => sc.Location);

        if (excludeScheduleId.HasValue)
        {
            query = query.Where(sc => sc.Id != excludeScheduleId.Value);
        }

        var overlappingSchedules = await query.ToListAsync();
        return overlappingSchedules.Select(ConvertToDto).ToList();
    }

    private ScheduleDto ConvertToDto(Schedule schedule)
    {
        return new ScheduleDto
        {
            Id = schedule.Id,
            SaintId = schedule.SaintId,
            Saint = schedule.Saint != null ? new SaintDto
            {
                Id = schedule.Saint.Id,
                Name = schedule.Saint.Name,
                Title = schedule.Saint.Title,
                IsActive = schedule.Saint.IsActive
            } : null,
            LocationId = schedule.LocationId,
            Location = schedule.Location != null ? new LocationDto
            {
                Id = schedule.Location.Id,
                Name = schedule.Location.Name,
                Address = schedule.Location.Address,
                City = schedule.Location.City,
                State = schedule.Location.State,
                Country = schedule.Location.Country
            } : null,
            StartDate = schedule.StartDate,
            EndDate = schedule.EndDate,
            Purpose = schedule.Purpose,
            Notes = schedule.Notes,
            ContactPerson = schedule.ContactPerson,
            ContactPhone = schedule.ContactPhone,
            CreatedAt = schedule.CreatedAt,
            UpdatedAt = schedule.UpdatedAt,
            IsCurrent = schedule.StartDate <= DateOnly.FromDateTime(DateTime.UtcNow) && schedule.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow),
            IsUpcoming = schedule.StartDate > DateOnly.FromDateTime(DateTime.UtcNow)
        };
    }
}
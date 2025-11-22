using Microsoft.EntityFrameworkCore;
using JainMunis.API.Data;
using JainMunis.API.Models.DTOs;
using JainMunis.API.Models.Entities;

namespace JainMunis.API.Services;

public class SaintService : ISaintService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthService _authService;

    public SaintService(ApplicationDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<(List<SaintDto> saints, int total)> GetSaintsAsync(int page, int limit, SearchParams? searchParams = null)
    {
        var query = _context.Saints.AsQueryable();

        // Apply filters
        if (searchParams != null)
        {
            if (!string.IsNullOrWhiteSpace(searchParams.Search))
            {
                var searchTerm = searchParams.Search.ToLower();
                query = query.Where(s => s.Name.ToLower().Contains(searchTerm) ||
                                       (s.Title != null && s.Title.ToLower().Contains(searchTerm)) ||
                                       (s.SpiritualLineage != null && s.SpiritualLineage.ToLower().Contains(searchTerm)));
            }

            if (searchParams.IsActive.HasValue)
            {
                query = query.Where(s => s.IsActive == searchParams.IsActive.Value);
            }
        }

        // Get total count
        var total = await query.CountAsync();

        // Apply pagination
        var saints = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var saintDtos = new List<SaintDto>();
        foreach (var saint in saints)
        {
            saintDtos.Add(await ConvertToDtoAsync(saint));
        }

        return (saintDtos, total);
    }

    public async Task<SaintDto?> GetSaintByIdAsync(Guid id)
    {
        var saint = await _context.Saints
            .Include(s => s.Schedules)
            .ThenInclude(sc => sc.Location)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (saint == null)
        {
            return null;
        }

        return await ConvertToDtoAsync(saint);
    }

    public async Task<SaintDto> CreateSaintAsync(CreateSaintRequest request)
    {
        var saint = new Saint
        {
            Name = request.Name,
            Title = request.Title,
            SpiritualLineage = request.SpiritualLineage,
            Bio = request.Bio,
            Phone = request.Phone,
            Email = request.Email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Saints.Add(saint);
        await _context.SaveChangesAsync();

        return await ConvertToDtoAsync(saint);
    }

    public async Task<SaintDto?> UpdateSaintAsync(Guid id, UpdateSaintRequest request)
    {
        var saint = await _context.Saints.FindAsync(id);
        if (saint == null)
        {
            return null;
        }

        var oldValues = new
        {
            saint.Name,
            saint.Title,
            saint.SpiritualLineage,
            saint.Bio,
            saint.Phone,
            saint.Email,
            saint.IsActive
        };

        if (!string.IsNullOrWhiteSpace(request.Name))
            saint.Name = request.Name;
        if (request.Title != null)
            saint.Title = request.Title;
        if (request.SpiritualLineage != null)
            saint.SpiritualLineage = request.SpiritualLineage;
        if (request.Bio != null)
            saint.Bio = request.Bio;
        if (request.Phone != null)
            saint.Phone = request.Phone;
        if (request.Email != null)
            saint.Email = request.Email;
        if (request.IsActive.HasValue)
            saint.IsActive = request.IsActive.Value;

        saint.UpdatedAt = DateTime.UtcNow;

        var newValues = new
        {
            saint.Name,
            saint.Title,
            saint.SpiritualLineage,
            saint.Bio,
            saint.Phone,
            saint.Email,
            saint.IsActive
        };

        await _context.SaveChangesAsync();

        // Log activity
        await _authService.LogActivityAsync(
            null, // TODO: Get current admin user ID
            "UPDATE_SAINT",
            "saint",
            saint.Id,
            System.Text.Json.JsonSerializer.Serialize(oldValues),
            System.Text.Json.JsonSerializer.Serialize(newValues),
            null,
            null
        );

        return await ConvertToDtoAsync(saint);
    }

    public async Task<bool> DeleteSaintAsync(Guid id)
    {
        var saint = await _context.Saints.FindAsync(id);
        if (saint == null)
        {
            return false;
        }

        saint.IsActive = false;
        saint.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log activity
        await _authService.LogActivityAsync(
            null, // TODO: Get current admin user ID
            "DELETE_SAINT",
            "saint",
            saint.Id,
            null,
            System.Text.Json.JsonSerializer.Serialize(new { IsActive = false }),
            null,
            null
        );

        return true;
    }

    public async Task<string> UpdateSaintPhotoAsync(Guid id, IFormFile photoFile)
    {
        await Task.CompletedTask;
        // TODO: Implement file upload logic
        // For now, return a placeholder URL
        return $"/uploads/saints/{id}/{Guid.NewGuid()}{Path.GetExtension(photoFile.FileName)}";
    }

    public async Task<List<SaintDto>> GetSaintsByCityAsync(string city)
    {
        var saints = await _context.Saints
            .Where(s => s.IsActive)
            .Where(s => s.Schedules.Any(sc => sc.Location.City.ToLower() == city.ToLower() &&
                                            sc.StartDate <= DateOnly.FromDateTime(DateTime.UtcNow) &&
                                            sc.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow)))
            .ToListAsync();

        var saintDtos = new List<SaintDto>();
        foreach (var saint in saints)
        {
            saintDtos.Add(await ConvertToDtoAsync(saint));
        }

        return saintDtos;
    }

    public async Task<List<SaintDto>> GetNearbySaintsAsync(decimal latitude, decimal longitude, int radiusKm)
    {
        await Task.CompletedTask;
        // TODO: Implement geospatial query
        // For now, return empty list
        return new List<SaintDto>();
    }

    private async Task<SaintDto> ConvertToDtoAsync(Saint saint)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var schedules = await _context.Schedules
            .Where(sc => sc.SaintId == saint.Id)
            .Include(sc => sc.Location)
            .Include(sc => sc.Creator)
            .ToListAsync();

        var currentSchedule = schedules.FirstOrDefault(sc => sc.StartDate <= today && sc.EndDate >= today);
        var upcomingSchedules = schedules.Where(sc => sc.StartDate > today).OrderBy(sc => sc.StartDate).Take(5).ToList();

        return new SaintDto
        {
            Id = saint.Id,
            Name = saint.Name,
            Title = saint.Title,
            SpiritualLineage = saint.SpiritualLineage,
            Bio = saint.Bio,
            PhotoUrl = saint.PhotoUrl,
            IsActive = saint.IsActive,
            CreatedAt = saint.CreatedAt,
            UpdatedAt = saint.UpdatedAt,
            CurrentSchedule = currentSchedule != null ? ConvertScheduleToDto(currentSchedule) : null,
            UpcomingSchedules = upcomingSchedules.Select(ConvertScheduleToDto).ToList()
        };
    }

    private ScheduleDto ConvertScheduleToDto(Schedule schedule)
    {
        return new ScheduleDto
        {
            Id = schedule.Id,
            SaintId = schedule.SaintId,
            Saint = new SaintDto
            {
                Id = schedule.Saint.Id,
                Name = schedule.Saint.Name,
                Title = schedule.Saint.Title,
                IsActive = schedule.Saint.IsActive
            },
            LocationId = schedule.LocationId,
            Location = new LocationDto
            {
                Id = schedule.Location.Id,
                Name = schedule.Location.Name,
                Address = schedule.Location.Address,
                City = schedule.Location.City,
                State = schedule.Location.State,
                Country = schedule.Location.Country
            },
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
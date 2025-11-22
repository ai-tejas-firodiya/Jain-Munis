using Microsoft.EntityFrameworkCore;
using JainMunis.API.Data;
using JainMunis.API.Models.DTOs;
using JainMunis.API.Models.Entities;

namespace JainMunis.API.Services;

public class LocationService : ILocationService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthService _authService;

    public LocationService(ApplicationDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<(List<LocationDto> locations, int total)> GetLocationsAsync(int page, int limit, SearchParams? searchParams = null)
    {
        var query = _context.Locations.AsQueryable();

        // Apply filters
        if (searchParams != null)
        {
            if (!string.IsNullOrWhiteSpace(searchParams.Search))
            {
                var searchTerm = searchParams.Search.ToLower();
                query = query.Where(l => l.Name.ToLower().Contains(searchTerm) ||
                                       l.Address.ToLower().Contains(searchTerm) ||
                                       l.City.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(searchParams.City))
            {
                query = query.Where(l => l.City.ToLower() == searchParams.City.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(searchParams.State))
            {
                query = query.Where(l => l.State != null && l.State.ToLower() == searchParams.State.ToLower());
            }
        }

        // Get total count
        var total = await query.CountAsync();

        // Apply pagination
        var locations = await query
            .OrderBy(l => l.City)
            .ThenBy(l => l.Name)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var locationDtos = new List<LocationDto>();
        foreach (var location in locations)
        {
            locationDtos.Add(await ConvertToDtoAsync(location));
        }

        return (locationDtos, total);
    }

    public async Task<LocationDto?> GetLocationByIdAsync(Guid id)
    {
        var location = await _context.Locations
            .Include(l => l.Schedules)
            .ThenInclude(sc => sc.Saint)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (location == null)
        {
            return null;
        }

        return await ConvertToDtoAsync(location);
    }

    public async Task<LocationDto> CreateLocationAsync(CreateLocationRequest request)
    {
        var location = new Location
        {
            Name = request.Name,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ContactPhone = request.ContactPhone,
            CreatedAt = DateTime.UtcNow
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        return await ConvertToDtoAsync(location);
    }

    public async Task<LocationDto?> UpdateLocationAsync(Guid id, UpdateLocationRequest request)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null)
        {
            return null;
        }

        var oldValues = new
        {
            location.Name,
            location.Address,
            location.City,
            location.State,
            location.PostalCode,
            location.Country,
            location.Latitude,
            location.Longitude,
            location.ContactPhone
        };

        if (!string.IsNullOrWhiteSpace(request.Name))
            location.Name = request.Name;
        if (!string.IsNullOrWhiteSpace(request.Address))
            location.Address = request.Address;
        if (!string.IsNullOrWhiteSpace(request.City))
            location.City = request.City;
        if (request.State != null)
            location.State = request.State;
        if (request.PostalCode != null)
            location.PostalCode = request.PostalCode;
        if (!string.IsNullOrWhiteSpace(request.Country))
            location.Country = request.Country;
        if (request.Latitude.HasValue)
            location.Latitude = request.Latitude.Value;
        if (request.Longitude.HasValue)
            location.Longitude = request.Longitude.Value;
        if (request.ContactPhone != null)
            location.ContactPhone = request.ContactPhone;

        var newValues = new
        {
            location.Name,
            location.Address,
            location.City,
            location.State,
            location.PostalCode,
            location.Country,
            location.Latitude,
            location.Longitude,
            location.ContactPhone
        };

        await _context.SaveChangesAsync();

        // Log activity
        await _authService.LogActivityAsync(
            null, // TODO: Get current admin user ID
            "UPDATE_LOCATION",
            "location",
            location.Id,
            System.Text.Json.JsonSerializer.Serialize(oldValues),
            System.Text.Json.JsonSerializer.Serialize(newValues),
            null,
            null
        );

        return await ConvertToDtoAsync(location);
    }

    public async Task<bool> DeleteLocationAsync(Guid id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null)
        {
            return false;
        }

        // Check if there are any schedules for this location
        var hasSchedules = await _context.Schedules.AnyAsync(sc => sc.LocationId == id);
        if (hasSchedules)
        {
            return false; // Cannot delete location with existing schedules
        }

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();

        // Log activity
        await _authService.LogActivityAsync(
            null, // TODO: Get current admin user ID
            "DELETE_LOCATION",
            "location",
            location.Id,
            System.Text.Json.JsonSerializer.Serialize(new { location.Name, location.City }),
            null,
            null,
            null
        );

        return true;
    }

    public async Task<List<string>> GetCitiesAsync(string? query = null)
    {
        var citiesQuery = _context.Locations.Select(l => l.City).Distinct();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var searchTerm = query.ToLower();
            citiesQuery = citiesQuery.Where(c => c.ToLower().Contains(searchTerm));
        }

        return await citiesQuery.OrderBy(c => c).Take(50).ToListAsync();
    }

    public async Task<List<LocationDto>> GetLocationsByCityAsync(string city)
    {
        var locations = await _context.Locations
            .Where(l => l.City.ToLower() == city.ToLower())
            .OrderBy(l => l.Name)
            .ToListAsync();

        var locationDtos = new List<LocationDto>();
        foreach (var location in locations)
        {
            locationDtos.Add(await ConvertToDtoAsync(location));
        }

        return locationDtos;
    }

    private async Task<LocationDto> ConvertToDtoAsync(Location location)
    {
        var schedules = await _context.Schedules
            .Where(sc => sc.LocationId == location.Id)
            .Include(sc => sc.Saint)
            .Include(sc => sc.Creator)
            .ToListAsync();

        return new LocationDto
        {
            Id = location.Id,
            Name = location.Name,
            Address = location.Address,
            City = location.City,
            State = location.State,
            PostalCode = location.PostalCode,
            Country = location.Country,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            ContactPhone = location.ContactPhone,
            CreatedAt = location.CreatedAt,
            Schedules = schedules.Select(ConvertScheduleToDto).ToList()
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
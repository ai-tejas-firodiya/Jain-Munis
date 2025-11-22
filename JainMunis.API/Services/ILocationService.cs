using JainMunis.API.Models.DTOs;

namespace JainMunis.API.Services;

public interface ILocationService
{
    Task<(List<LocationDto> locations, int total)> GetLocationsAsync(int page, int limit, SearchParams? searchParams = null);
    Task<LocationDto?> GetLocationByIdAsync(Guid id);
    Task<LocationDto> CreateLocationAsync(CreateLocationRequest request);
    Task<LocationDto?> UpdateLocationAsync(Guid id, UpdateLocationRequest request);
    Task<bool> DeleteLocationAsync(Guid id);
    Task<List<string>> GetCitiesAsync(string? query = null);
    Task<List<LocationDto>> GetLocationsByCityAsync(string city);
}
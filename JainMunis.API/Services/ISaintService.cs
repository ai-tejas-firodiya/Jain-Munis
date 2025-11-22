using JainMunis.API.Models.DTOs;

namespace JainMunis.API.Services;

public interface ISaintService
{
    Task<(List<SaintDto> saints, int total)> GetSaintsAsync(int page, int limit, SearchParams? searchParams = null);
    Task<SaintDto?> GetSaintByIdAsync(Guid id);
    Task<SaintDto> CreateSaintAsync(CreateSaintRequest request);
    Task<SaintDto?> UpdateSaintAsync(Guid id, UpdateSaintRequest request);
    Task<bool> DeleteSaintAsync(Guid id);
    Task<string> UpdateSaintPhotoAsync(Guid id, IFormFile photoFile);
    Task<List<SaintDto>> GetSaintsByCityAsync(string city);
    Task<List<SaintDto>> GetNearbySaintsAsync(decimal latitude, decimal longitude, int radiusKm);
}
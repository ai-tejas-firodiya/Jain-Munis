using JainMunis.API.Models.DTOs;

namespace JainMunis.API.Services;

public interface IScheduleService
{
    Task<(List<ScheduleDto> schedules, int total)> GetSchedulesAsync(int page, int limit, SearchParams? searchParams = null);
    Task<ScheduleDto?> GetScheduleByIdAsync(Guid id);
    Task<ScheduleDto> CreateScheduleAsync(CreateScheduleRequest request, Guid? createdBy);
    Task<ScheduleDto?> UpdateScheduleAsync(Guid id, UpdateScheduleRequest request, Guid? updatedBy);
    Task<bool> DeleteScheduleAsync(Guid id);
    Task<List<ScheduleDto>> GetCurrentSchedulesAsync(SearchParams? searchParams = null);
    Task<List<ScheduleDto>> GetUpcomingSchedulesAsync(SearchParams? searchParams = null);
    Task<List<ScheduleDto>> GetSchedulesBySaintAsync(Guid saintId);
    Task<List<ScheduleDto>> CheckOverlapsAsync(Guid saintId, DateOnly startDate, DateOnly endDate, Guid? excludeScheduleId = null);
}
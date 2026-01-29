using YC4.DTOs;
using YC4.Entity;

namespace YC4.Interfaces
{
    public interface IEventService
    {
        Task<Guid> CreateEventAsync(CreateEventDto dto);
        Task<bool> UpdateEventAsync(Guid id, CreateEventDto dto);
        Task<List<Event>> GetAllEventsAsync();
    }
}

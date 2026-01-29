using YC4.DTOs;

namespace YC4.Interfaces
{
    public interface IEventService
    {
        // Lấy toàn bộ sự kiện kèm thông tin tổng quát
        Task<IEnumerable<object>> GetAllEventsAsync();

        // Lấy danh sách ghế còn trống của 1 sự kiện cụ thể
        Task<IEnumerable<object>> GetAvailableSeatsAsync(Guid eventId);

        // Tạo sự kiện mới và tự động sinh ghế
        Task<Guid> CreateEventAsync(CreateEventDto dto);

        // Cập nhật sự kiện (bao gồm cả cấu hình lại ghế)
        Task<bool> UpdateEventAsync(Guid id, CreateEventDto dto);

        // Xóa sự kiện
        Task<bool> DeleteEventAsync(Guid id);
    }
}
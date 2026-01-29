using YC4.DTOs;

namespace YC4.Interfaces
{
    public interface IOrderService
    {
        // Trả về object chứa thông tin chi tiết sau khi đặt để Controller phản hồi
        Task<object> BookTicketsAsync(int userId, BookingRequest request);
    }
}
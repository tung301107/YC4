using Microsoft.EntityFrameworkCore;
using YC4.Data;
using YC4.Interfaces;
using YC4.Entity;

namespace YC4.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPriceCalculator _priceCalculator; // Dịch vụ Transient

        public OrderService(ApplicationDbContext context, IPriceCalculator priceCalculator)
        {
            _context = context;
            _priceCalculator = priceCalculator;
        }

        public async Task<Guid> PlaceOrderAsync(int userId, Guid eventId, List<Guid> seatIds)
        {
            // Sử dụng Transaction để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Lấy thông tin ghế và kiểm tra tính khả dụng
                var availableSeats = await _context.Seats
                    .Where(s => seatIds.Contains(s.SeatId) && s.IsAvailable && s.EventId == eventId)
                    .ToListAsync();

                if (availableSeats.Count != seatIds.Count)
                {
                    throw new Exception("Một số ghế đã bị đặt hoặc không tồn tại trong sự kiện này.");
                }

                // 2. Sử dụng Transient PriceCalculator để tính tổng tiền (An toàn hơn từ Server)
                decimal totalAmount = _priceCalculator.CalculateTotal(availableSeats.Select(s => s.Price));

                var orderId = Guid.NewGuid();

                // 3. Tạo đối tượng Order
                var newOrder = new Order
                {
                    OrderId = orderId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    Tickets = new List<Ticket>()
                };

                // 4. Duyệt qua từng ghế để đánh dấu đã đặt và tạo vé chi tiết
                foreach (var seat in availableSeats)
                {
                    seat.IsAvailable = false; // Chặn người khác đặt ghế này

                    newOrder.Tickets.Add(new Ticket
                    {
                        TicketId = Guid.NewGuid(),
                        OrderId = orderId,
                        SeatId = seat.SeatId,
                        PriceAtBooking = seat.Price // Lưu lại giá gốc tại thời điểm mua
                    });
                }

                // 5. Lưu vào Database
                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                // 6. Hoàn tất giao dịch
                await transaction.CommitAsync();

                return orderId;
            }
            catch (Exception ex)
            {
                // Nếu có bất kỳ lỗi nào, hoàn tác mọi thay đổi trong DB
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi quá trình đặt vé: {ex.Message}");
            }
        }
    }
}
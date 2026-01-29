using Microsoft.EntityFrameworkCore;
using YC4.Data;
using YC4.Interfaces;
using YC4.Entity;
using YC4.DTOs;

namespace YC4.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPriceCalculator _priceCalculator;

        public OrderService(ApplicationDbContext context, IPriceCalculator priceCalculator)
        {
            _context = context;
            _priceCalculator = priceCalculator;
        }

        public async Task<object> BookTicketsAsync(int userId, BookingRequest request)
        {
            // Sử dụng Transaction để đảm bảo nếu lưu Ticket lỗi thì Order cũng không được tạo
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var selectedSeats = await _context.Seats
                    .Where(s => request.SelectedSeatIds.Contains(s.SeatId) && s.IsAvailable && s.EventId == request.EventId)
                    .ToListAsync();

                if (selectedSeats.Count != request.SelectedSeatIds.Count)
                    throw new Exception("Một số ghế đã bị đặt hoặc không tồn tại.");

                var totalAmount = _priceCalculator.CalculateTotal(selectedSeats.Select(s => s.Price));
                var orderId = Guid.NewGuid();

                var newOrder = new Order
                {
                    OrderId = orderId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    // Quan trọng: Phải khởi tạo List Tickets nếu trong Entity chưa khởi tạo
                    Tickets = new List<Ticket>()
                };

                foreach (var seat in selectedSeats)
                {
                    seat.IsAvailable = false; // Đánh dấu ghế đã bán
                    newOrder.Tickets.Add(new Ticket
                    {
                        TicketId = Guid.NewGuid(),
                        OrderId = orderId,
                        SeatId = seat.SeatId,
                        PriceAtBooking = seat.Price
                    });
                }

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new
                {
                    OrderId = orderId,
                    TotalAmount = totalAmount,
                    TicketCount = selectedSeats.Count,
                    Message = "Đặt vé thành success!"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.Message);
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using YC4.Attributes;
using YC4.Data;
using YC4.Interfaces;
using YC4.DTOs;
using Microsoft.EntityFrameworkCore;
using YC4.Entity;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IPriceCalculator _priceCalculator;
    private readonly ApplicationDbContext _context;

    public OrderController(
        IOrderService orderService,
        IPriceCalculator priceCalculator,
        ApplicationDbContext context)
    {
        _orderService = orderService;
        _priceCalculator = priceCalculator;
        _context = context;
    }

    [HasPermission("BOOK")]
    [HttpPost("book")]
    public async Task<IActionResult> BookTickets([FromBody] BookingRequest request)
    {
        // 1. Lấy UserId từ Session
        var userIdString = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized(new { Message = "Vui lòng đăng nhập để thực hiện đặt vé." });
        }

        // Chuyển đổi UserId sang kiểu int (vì User.cs của bạn Id là int)
        int currentUserId = int.Parse(userIdString);

        try
        {
            // 2. Kiểm tra ghế khả dụng
            var selectedSeats = await _context.Seats
                .Where(s => request.SelectedSeatIds.Contains(s.SeatId) && s.IsAvailable)
                .ToListAsync();

            if (selectedSeats.Count != request.SelectedSeatIds.Count)
                return BadRequest("Một số ghế đã bị đặt hoặc không tồn tại.");

            // 3. Tính tổng tiền
            var totalAmount = _priceCalculator.CalculateTotal(selectedSeats.Select(s => s.Price));

            // 4. Gọi Service để đặt vé
            // Lưu ý: currentUserId được truyền vào đây thay vì lấy từ request
            var orderId = await _orderService.PlaceOrderAsync(
                currentUserId, // Tự động điền từ Session
                request.EventId,
                request.SelectedSeatIds
            );

            return Ok(new
            {
                OrderId = orderId,
                CustomerName = HttpContext.Session.GetString("FullName"), // Lấy tên từ session trả về cho UI
                BookingTime = DateTime.Now,
                TotalAmount = totalAmount,
                TicketCount = selectedSeats.Count,
                Message = "Đặt vé thành công!"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YC4.DTOs;
using YC4.Interfaces;

namespace YC4.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Authorize(Policy = "CanBookTicket")]
        [HttpPost("book")]
        public async Task<IActionResult> BookTickets([FromBody] BookingRequest request)
        {
            // 1. Trích xuất UserId từ Token đã được xác thực
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized(new { Message = "Token không hợp lệ hoặc đã hết hạn." });
            }

            int userId = int.Parse(userIdClaim.Value);

            try
            {
                // 2. Gọi Service xử lý đặt vé (Logic kiểm tra ghế, tính tiền nằm trong Service)
                var result = await _orderService.BookTicketsAsync(userId, request);

                return Ok(new
                {
                    Data = result,
                    Message = "Đặt vé thành công!"
                });
            }
            catch (Exception ex)
            {
                // Trả về lỗi nếu ghế đã bị giữ hoặc có vấn đề khác
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
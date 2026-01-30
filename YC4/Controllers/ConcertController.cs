using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YC4.DTOs;
using YC4.Interfaces;

namespace YC4.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConcertController : ControllerBase
    {
        private readonly IEventService _eventService;

        public ConcertController(IEventService eventService)
        {
            _eventService = eventService;
        }

        // 1. Lấy danh sách tất cả sự kiện
        [Authorize(Policy = "CanViewConcert")]
        [HttpGet("events")]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _eventService.GetAllEventsAsync();
            return Ok(events);
        }

        // 2. Tạo sự kiện mới
        [Authorize(Policy = "CanCreateConcert")]
        [HttpPost("events")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            try
            {
                var id = await _eventService.CreateEventAsync(dto);
                return Ok(new { EventId = id, Message = "Tạo sự kiện thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // 3. Xem danh sách ghế trống của một sự kiện
        [Authorize(Policy = "CanViewSeats")]
        [HttpGet("events/{eventId}/available-seats")]
        public async Task<IActionResult> GetAvailableSeats(Guid eventId)
        {
            var seats = await _eventService.GetAvailableSeatsAsync(eventId);
            return Ok(seats);
        }

        // 4. Cập nhật thông tin sự kiện
        [Authorize(Policy = "CanUpdateConcert")]
        [HttpPut("events/{id}")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] CreateEventDto dto)
        {
            var result = await _eventService.UpdateEventAsync(id, dto);
            if (!result) return NotFound("Không tìm thấy sự kiện để cập nhật.");
            return Ok(new { Message = "Cập nhật thành công." });
        }
    }
}
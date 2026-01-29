using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YC4.Attributes;
using YC4.Data;
using YC4.DTOs;
using YC4.Entity;
using YC4.Interfaces;

namespace YC4.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConcertController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly ApplicationDbContext _context;

    public ConcertController(
        IEventService eventService,
        ApplicationDbContext context)
    {
        _eventService = eventService;
        _context = context;
    }

    // 1. Lấy danh sách tất cả sự kiện
    [HasPermission("CONCERT_view")]
    [HttpGet("events")]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Events
            .Select(e => new {
                e.Id,
                e.Name,
                e.DateTime,
                e.Description,
                TotalSeats = e.TotalSeats,
                AvailableSeats = e.Seats.Count(s => s.IsAvailable)
            })
            .ToListAsync();

        return Ok(events);
    }

    // 2. Tạo sự kiện mới
    [HasPermission("CONCERT_CREATE")]
    [HttpPost("events")]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
    {
        if (dto.Rows == null || !dto.Rows.Any())
            return BadRequest("Danh sách hàng không được để trống.");

        try
        {
            var id = await _eventService.CreateEventAsync(dto);
            return Ok(new { EventId = id, Message = "Tạo sự kiện thành công." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi: {ex.Message}");
        }
    }

    // 3. Cập nhật sự kiện
    [HasPermission("CONCERT_UPDATE")]
    [HttpPut("events/{id}")]
    public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] CreateEventDto dto)
    {
        var result = await _eventService.UpdateEventAsync(id, dto);
        if (!result) return NotFound("Không tìm thấy sự kiện.");
        return Ok("Cập nhật thành công.");
    }

    [HasPermission("Available_Seat")]
    [HttpGet("events/{eventId}/available-seats")]
    public async Task<IActionResult> GetAvailableSeats(Guid eventId)
    {
        var seats = await _context.Seats
            .Where(s => s.EventId == eventId && s.IsAvailable)
            .OrderBy(s => s.RowName).ThenBy(s => s.SeatNumber)
            .ToListAsync();
        return Ok(seats);
    }

}
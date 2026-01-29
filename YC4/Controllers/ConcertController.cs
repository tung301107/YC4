using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YC4.DTOs;
using YC4.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class ConcertController : ControllerBase
{
    private readonly IEventService _eventService;
    public ConcertController(IEventService eventService) => _eventService = eventService;

    [Authorize(Policy = "CanViewConcert")]
    [HttpGet("events")]
    public async Task<IActionResult> GetEvents() => Ok(await _eventService.GetAllEventsAsync());

    [Authorize(Policy = "CanCreateConcert")]
    [HttpPost("events")]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
    {
        var id = await _eventService.CreateEventAsync(dto);
        return Ok(new { EventId = id, Message = "Thành công" });
    }

    [Authorize(Policy = "CanViewAvailableSeats")]
    [HttpGet("events/{eventId}/available-seats")]
    public async Task<IActionResult> GetAvailableSeats(Guid eventId)
        => Ok(await _eventService.GetAvailableSeatsAsync(eventId));
}
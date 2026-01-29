using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YC4.DTOs;
using YC4.Entity;
using YC4.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    public OrderController(IOrderService orderService) => _orderService = orderService;

    [Authorize(Policy = "CanBookTicket")]
    [HttpPost("book")]
    public async Task<IActionResult> BookTickets([FromBody] BookingRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
        try
        {
            var result = await _orderService.BookTicketsAsync(userId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
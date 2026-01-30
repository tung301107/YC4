using Microsoft.AspNetCore.Mvc;
using YC4.Interfaces;

namespace YC4.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthInterface _authService;

        public AuthController(IAuthInterface authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var result = await _authService.Login(loginRequest);
            if (!result.Success)
            {
                return Unauthorized(new { Success = false, Message = result.Message });
            }
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            var result = await _authService.Register(registerRequest);
            if (!result.Success)
            {
                return BadRequest(new { Success = false, Message = result.Message });
            }
            return Ok(result);
        }
    }
}

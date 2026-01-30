using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YC4.DTOs;
using YC4.Interfaces;

namespace YC4.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var token = await _accountService.LoginAsync(request);
            if (token == null)
                return Unauthorized(new { Message = "Tài khoản hoặc mật khẩu không chính xác!" });

            return Ok(new { Token = token, Message = "Đăng nhập thành công" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            var result = await _accountService.RegisterAsync(request);
            if (!result) return BadRequest(new { Message = "Đăng ký thất bại hoặc tên tài khoản đã tồn tại!" });
            return Ok(new { Message = "Đăng ký tài công!" });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var profile = await _accountService.GetProfileAsync(int.Parse(userIdStr));
            return profile == null ? NotFound() : Ok(profile);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var result = await _accountService.UpdateProfileAsync(int.Parse(userIdStr), request);
            return result ? Ok(new { Message = "Cập nhật thông tin thành công!" }) : NotFound();
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var result = await _accountService.ChangePasswordAsync(int.Parse(userIdStr), request);
            return result ? Ok(new { Message = "Đổi mật khẩu thành công!" }) : BadRequest("Mật khẩu cũ không chính xác.");
        }
    }
}
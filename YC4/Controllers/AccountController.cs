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
            if (token == null) return Unauthorized(new { Message = "Sai tài khoản hoặc mật khẩu!" });
            return Ok(new { Token = token, Message = "Đăng nhập thành công" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            var result = await _accountService.RegisterAsync(request);
            if (!result) return BadRequest(new { Message = "Tên tài khoản đã tồn tại!" });
            return Ok(new { Message = "Đăng ký thành công!" });
        }

        // Với JWT, Logout thường xử lý ở Client (Xóa Token), 
        // ở Server chỉ cần trả về OK hoặc xóa Cookie nếu dùng HttpOnly Cookie.
        [HttpPost("logout")]
        public IActionResult Logout() => Ok(new { Message = "Đã đăng xuất (Hãy xóa token ở phía Client)" });

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var profile = await _accountService.GetProfileAsync(userId);
            return profile == null ? NotFound() : Ok(profile);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _accountService.UpdateProfileAsync(userId, request);
            return result ? Ok(new { Message = "Cập nhật thành công!" }) : NotFound();
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _accountService.ChangePasswordAsync(userId, request);
            return result ? Ok(new { Message = "Đổi mật khẩu thành công!" }) : BadRequest("Mật khẩu cũ không đúng.");
        }
    }
}
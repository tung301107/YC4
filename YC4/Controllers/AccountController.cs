using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YC4.Data;
using YC4.DTOs;
using YC4.Entity; // BẮT BUỘC phải có để nhận diện lớp User và UserRole

namespace YC4.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            // 1. Kiểm tra tồn tại
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest(new { Message = "Tên tài khoản đã tồn tại!" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. Tạo User
                var newUser = new User
                {
                    Username = request.Username,
                    Password = request.Password, // Nên dùng thư viện BCrypt để hash
                    FullName = request.FullName,
                    // Khởi tạo danh sách tránh lỗi NullReferenceException
                    UserRoles = new List<UserRole>()
                };

                // 3. Gán Role Customer (ID = 2)
                // Lưu ý: Chỉ cần gán RoleId, EF sẽ tự hiểu đây là bản ghi bảng trung gian
                newUser.UserRoles.Add(new UserRole
                {
                    RoleId = 2
                });

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { Message = "Đăng ký khách hàng thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Trả về lỗi chi tiết để debug dễ hơn trong quá trình phát triển
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

            if (user != null)
            {
                // Lưu thông tin vào Session
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("FullName", user.FullName);

                return Ok(new
                {
                    Message = "Đăng nhập thành công",
                    User = user.FullName
                });
            }

            return Unauthorized(new { Message = "Sai tài khoản hoặc mật khẩu!" });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { Message = "Đã đăng xuất" });
        }
        // Lấy thông tin cá nhân
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null) return NotFound();

            return Ok(new { user.Username, user.FullName, user.Email, user.PhoneNumber });
        }

        // Cập nhật thông tin cá nhân
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null) return NotFound();

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Cập nhật thông tin thành công!" });
        }

        // Đổi mật khẩu
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null || user.Password != request.OldPassword)
                return BadRequest("Mật khẩu cũ không chính xác.");

            user.Password = request.NewPassword;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Đổi mật khẩu thành công!" });
        }
    }
}
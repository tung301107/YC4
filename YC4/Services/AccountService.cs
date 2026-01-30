using Microsoft.EntityFrameworkCore;
using YC4.Data;
using YC4.DTOs;
using YC4.Entity;
using YC4.Interfaces;

namespace YC4.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;

        public AccountService(ApplicationDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<string?> LoginAsync(LoginDto request)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.UserFunctions).ThenInclude(uf => uf.Function)
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

            if (user == null) return null;

            // 1. Lấy danh sách RoleCode
            var roles = user.UserRoles.Select(ur => ur.Role.RoleCode).ToList();

            // 2. Lấy danh sách Permission (Gộp từ Role và User trực tiếp)
            var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
            var rolePermissions = await _context.RoleFunctions
                .Where(rf => roleIds.Contains(rf.RoleId))
                .Select(rf => rf.Function.FunctionCode).ToListAsync();

            var directPermissions = user.UserFunctions.Select(uf => uf.Function.FunctionCode);

            var allPermissions = rolePermissions.Union(directPermissions).Distinct().ToList();

            // 3. Gọi JwtService sinh Token
            return _jwtService.GenerateToken(user, roles, allPermissions);
        }

        public async Task<bool> RegisterAsync(RegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username)) return false;

            var newUser = new User
            {
                Username = request.Username,
                Password = request.Password, // Lưu ý: Nên Hash mật khẩu ở đây
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber
            };

            _context.Users.Add(newUser);

            // Gán role mặc định (ví dụ ID = 2 là Customer/User)
            var result = await _context.SaveChangesAsync() > 0;
            if (result)
            {
                _context.UserRoles.Add(new UserRole { UserId = newUser.Id, RoleId = 2 });
                await _context.SaveChangesAsync();
            }
            return result;
        }

        public async Task<object?> GetProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user == null ? null : new { user.Username, user.FullName, user.Email, user.PhoneNumber };
        }

        public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Password != request.OldPassword) return false;

            user.Password = request.NewPassword;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
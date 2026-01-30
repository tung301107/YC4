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
        private readonly IUserInterface _userInterface;

        public AccountService(ApplicationDbContext context, IJwtService jwtService, IUserInterface userInterface)
        {
            _context = context;
            _jwtService = jwtService;
            _userInterface = userInterface;
        }

        public async Task<string?> LoginAsync(LoginDto request)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.UserFunctions).ThenInclude(uf => uf.Function)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) 
                return null;

            var roles = await _userInterface.GetUserRolesAsync(user.UserId);
            var permissions = await _userInterface.GetUserFunctionsAsync(user.UserId);

            return _jwtService.GenerateToken(user, roles, permissions);
        }

        public async Task<bool> RegisterAsync(RegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username)) return false;

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                IsActive = true
            };

            _context.Users.Add(newUser);
            var result = await _context.SaveChangesAsync() > 0;
            
            if (result)
            {
                // Assign default role (ID = 3 is User according to SeedData)
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
                if (userRole != null)
                {
                    _context.UserRoles.Add(new User_Role { UserId = newUser.UserId, RoleId = userRole.RoleId });
                    await _context.SaveChangesAsync();
                }
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
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash)) 
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
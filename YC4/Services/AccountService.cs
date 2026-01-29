using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using YC4.Data;
using YC4.DTOs;
using YC4.Entity;
using YC4.Interfaces;

namespace YC4.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AccountService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> LoginAsync(LoginDto request)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.UserFunctions).ThenInclude(uf => uf.Function)
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

            if (user == null) return null;

            // Lấy quyền từ Role và đặc cách
            var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
            var rolePermissions = await _context.RoleFunctions
                .Where(rf => roleIds.Contains(rf.RoleId))
                .Select(rf => rf.Function.FunctionCode).ToListAsync();
            var userPermissions = user.UserFunctions.Select(uf => uf.Function.FunctionCode);
            var allPermissions = rolePermissions.Union(userPermissions).Distinct();

            // Tạo Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            foreach (var role in user.UserRoles.Select(ur => ur.Role.RoleCode)) claims.Add(new Claim(ClaimTypes.Role, role));
            foreach (var perm in allPermissions) claims.Add(new Claim("Permission", perm));

            // Sinh Token
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "Chuoi_Key_Bi_Mat_Cua_Ban_Phai_Du_Dai_32_Ky_Tu");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        public async Task<bool> RegisterAsync(RegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username)) return false;
            var newUser = new User
            {
                Username = request.Username,
                Password = request.Password,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                UserRoles = new List<UserRole> { new UserRole { RoleId = 2 } }
            };
            _context.Users.Add(newUser);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<object> GetProfileAsync(int userId)
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
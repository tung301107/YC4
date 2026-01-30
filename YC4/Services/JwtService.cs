using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using YC4.Entity;
using YC4.Interfaces;

namespace YC4.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public string GenerateToken(User user, List<string> roles, List<string> permissions)
        {
            // 1. Khởi tạo danh sách Claims (Thông tin định danh)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // 2. Thêm các Vai trò (Roles) vào Claim
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 3. Thêm các Quyền chức năng (Permissions) vào Claim
            // Lưu ý: Key "Permission" phải khớp với RequireClaim trong Program.cs
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }

            // 4. Tạo khóa ký và thuật toán
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 5. Cấu hình Token
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: creds
            );

            // 6. Trả về chuỗi Token đã ký
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using YC4.Data;
using YC4.Interfaces;

namespace YC4.Services
{
    public class AuthService : IAuthInterface
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IUserInterface _userInterface;

        public AuthService(ApplicationDbContext context, IJwtService jwtService, IUserInterface userInterface)
        {
            _context = context;
            _jwtService = jwtService;
            _userInterface = userInterface;
        }

        public async Task<LoginResponse> Login(LoginRequest loginRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginRequest.UserName || u.Email == loginRequest.UserName);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                return new LoginResponse { Success = false, Message = "Invalid username or password" };
            }

            var roles = await _userInterface.GetUserRolesAsync(user.UserId);
            var permissions = await _userInterface.GetUserFunctionsAsync(user.UserId);

            var token = _jwtService.GenerateToken(user, roles, permissions);

            return new LoginResponse
            {
                Success = true,
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(60),
                User = new UserInfo
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    Roles = roles,
                    Permissions = permissions
                },
                Message = "Login successful"
            };
        }

        public async Task<LoginResponse> Register(RegisterRequest registerRequest)
        {
            if (await _userInterface.ExistsAsync(registerRequest.Username))
                return new LoginResponse { Success = false, Message = "Username already exists" };

            var user = new Entity.User
            {
                Username = registerRequest.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                FullName = registerRequest.FullName,
                Email = registerRequest.Email,
                IsActive = true
            };

            var newUser = await _userInterface.CreateAsync(user);
            
            // Assign default role "User"
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
            if (userRole != null)
            {
                await _userInterface.AssignRoleAsync(newUser.UserId, userRole.RoleId);
            }

            return new LoginResponse { Success = true, Message = "Registration successful" };
        }
    }
}

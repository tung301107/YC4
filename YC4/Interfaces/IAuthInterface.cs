using YC4.Entity;

namespace YC4.Interfaces
{
    public interface IAuthInterface
    {
        Task<LoginResponse> Login(LoginRequest loginRequest);
        Task<LoginResponse> Register(RegisterRequest registerRequest);
    }

    public class LoginRequest
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public DateTime Expiration { get; set; }
        public UserInfo? User { get; set; }
        public string? Message { get; set; }
    }

    public class UserInfo
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }
}

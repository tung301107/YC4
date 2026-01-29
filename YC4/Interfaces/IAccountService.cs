using YC4.DTOs;

namespace YC4.Interfaces
{
    public interface IAccountService
    {
        Task<string> LoginAsync(LoginDto request);
        Task<bool> RegisterAsync(RegisterDto request);
        Task<object> GetProfileAsync(int userId);
        Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto request);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto request);
    }
}
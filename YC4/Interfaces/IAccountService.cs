using YC4.DTOs;

namespace YC4.Interfaces
{
    public interface IAccountService
    {
        Task<object?> GetProfileAsync(int userId);
        Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto request);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto request);
    }
}
// File: YC4.Interfaces/IUserService.cs
using YC4.DTOs;
using YC4.Entity;

namespace YC4.Interfaces
{
    public interface IUserService
    {
        // Cho người dùng (Profile)
        Task<User?> GetProfileAsync(int userId);
        Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);

        // Cho Admin quản lý User
        Task<List<User>> GetAllUsersAsync();
        Task<bool> AdminUpdateUserAsync(int userId, UpdateProfileDto dto);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ResetPasswordAsync(int userId, string newPassword);

        // Quản lý Quyền (Permissions/Functions)
        Task<List<Function>> GetAllFunctionsAsync();
        Task<bool> AssignRoleToUserAsync(int userId, int roleId);
        Task<bool> AssignPermissionToUserAsync(int userId, int? functionId, string? functionCode);
        Task<bool> AssignPermissionToRoleAsync(int roleId, int? functionId, string? functionCode);
    }
}
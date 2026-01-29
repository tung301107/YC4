// File: YC4.Services/UserService.cs
using Microsoft.EntityFrameworkCore;
using YC4.Data;
using YC4.DTOs;
using YC4.Entity;
using YC4.Interfaces;

namespace YC4.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context) => _context = context;

        // --- Logic cho Người dùng ---
        public async Task<User?> GetProfileAsync(int userId)
            => await _context.Users.FindAsync(userId);

        public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Password != oldPassword) return false;

            user.Password = newPassword;
            return await _context.SaveChangesAsync() > 0;
        }

        // --- Logic cho Admin ---
        public async Task<List<User>> GetAllUsersAsync()
            => await _context.Users.Include(u => u.UserRoles).ToListAsync();

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            _context.Users.Remove(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Function>> GetAllFunctionsAsync()
            => await _context.Functions.ToListAsync();

        public async Task<bool> AssignPermissionToUserAsync(int userId, int? functionId, string? functionCode)
        {
            var function = await _context.Functions
                .FirstOrDefaultAsync(f => f.Id == functionId || f.FunctionCode == functionCode);

            if (function == null) return false;

            var exists = await _context.UserFunctions
                .AnyAsync(uf => uf.UserId == userId && uf.FunctionId == function.Id);

            if (!exists)
            {
                _context.UserFunctions.Add(new UserFunction { UserId = userId, FunctionId = function.Id });
                return await _context.SaveChangesAsync() > 0;
            }
            return true;
        }

        // Tương tự triển khai cho AssignPermissionToRoleAsync và AdminUpdateUserAsync...
        public async Task<bool> AdminUpdateUserAsync(int userId, UpdateProfileDto dto) => await UpdateProfileAsync(userId, dto);
        public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            user.Password = newPassword;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
        {
            var user = await _context.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;
            if (!user.UserRoles.Any(r => r.RoleId == roleId))
            {
                user.UserRoles.Add(new UserRole { RoleId = roleId });
                return await _context.SaveChangesAsync() > 0;
            }
            return true;
        }

        public async Task<bool> AssignPermissionToRoleAsync(int roleId, int? functionId, string? functionCode)
        {
            var function = await _context.Functions.FirstOrDefaultAsync(f => f.Id == functionId || f.FunctionCode == functionCode);
            if (function == null) return false;
            var exists = await _context.RoleFunctions.AnyAsync(rf => rf.RoleId == roleId && rf.FunctionId == function.Id);
            if (!exists)
            {
                _context.RoleFunctions.Add(new RoleFunction { RoleId = roleId, FunctionId = function.Id });
                return await _context.SaveChangesAsync() > 0;
            }
            return true;
        }
    }
}
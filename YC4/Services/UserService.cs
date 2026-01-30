using Microsoft.EntityFrameworkCore;
using YC4.Data;
using YC4.Entity;
using YC4.Interfaces;

namespace YC4.Services
{
    public class UserService : IUserInterface
    {
        private readonly ApplicationDbContext _context;
        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.UserFunctions)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.UserFunctions)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<List<YC4.DTOs.UserDto>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Where(u => u.IsActive)
                .Select(u => new YC4.DTOs.UserDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Roles = u.UserRoles.Select(ur => ur.Role.RoleName).ToList()
                })
                .ToListAsync();
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync();
        }

        public async Task<List<Role>> GetUserRoleObjectsAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role)
                .ToListAsync();
        }

        public async Task<bool> HasRoleAsync(int userId, string roleName)
        {
            return await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.Role.RoleName == roleName);
        }

        public async Task<bool> AssignRoleAsync(int userId, int roleId)
        {
            var exists = await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (exists) return false;

            _context.UserRoles.Add(new User_Role { UserId = userId, RoleId = roleId });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveRoleAsync(int userId, int roleId)
        {
            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (userRole == null) return false;
            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<string>> GetUserFunctionsAsync(int userId)
        {
            var directFunctions = await _context.UserFunctions
                .Where(uf => uf.UserId == userId)
                .Select(uf => uf.Function.FunctionCode)
                .ToListAsync();

            var roleFunctions = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RoleFunctions)
                .Select(rf => rf.Function.FunctionCode)
                .ToListAsync();

            return directFunctions.Union(roleFunctions).Distinct().ToList();
        }

        public async Task<List<Function>> GetUserFunctionObjectsAsync(int userId)
        {
            var directFunctions = await _context.UserFunctions
                .Where(uf => uf.UserId == userId)
                .Select(uf => uf.Function)
                .ToListAsync();

            var roleFunctions = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RoleFunctions)
                .Select(rf => rf.Function)
                .ToListAsync();

            return directFunctions.Union(roleFunctions).GroupBy(f => f.FunctionId).Select(g => g.First()).ToList();
        }

        public async Task<bool> HasFunctionAsync(int userId, string functionCode)
        {
            var direct = await _context.UserFunctions.AnyAsync(uf => uf.UserId == userId && uf.Function.FunctionCode == functionCode);
            if (direct) return true;

            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .AnyAsync(ur => ur.Role.RoleFunctions.Any(rf => rf.Function.FunctionCode == functionCode));
        }

        public async Task<bool> AssignFunctionAsync(int userId, int functionId)
        {
            var exists = await _context.UserFunctions.AnyAsync(uf => uf.UserId == userId && uf.FunctionId == functionId);
            if (exists) return false;

            _context.UserFunctions.Add(new User_Function { UserId = userId, FunctionId = functionId });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFunctionAsync(int userId, int functionId)
        {
            var uf = await _context.UserFunctions.FirstOrDefaultAsync(uf => uf.UserId == userId && uf.FunctionId == functionId);
            if (uf == null) return false;
            _context.UserFunctions.Remove(uf);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
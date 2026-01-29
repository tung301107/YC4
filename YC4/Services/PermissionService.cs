using Microsoft.EntityFrameworkCore;
using YC4.Interfaces;
using YC4.Data;

namespace YC4.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;
        public PermissionService(ApplicationDbContext context) => _context = context;

        public async Task<bool> CheckUserPermissionAsync(int userId, string functionCode)
        {
            // 1. Kiểm tra trong quyền đặc cách (UserFunction)
            var hasDirectPermission = await _context.UserFunctions
                .AnyAsync(uf => uf.UserId == userId && uf.Function.FunctionCode == functionCode);
            if (hasDirectPermission) return true;

            // 2. Kiểm tra trong các vai trò của User (UserRole -> RoleFunction)
            var hasRolePermission = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RoleFunctions)
                .AnyAsync(rf => rf.Function.FunctionCode == functionCode);

            return hasRolePermission;
        }
    }
}

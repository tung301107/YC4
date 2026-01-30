using Microsoft.EntityFrameworkCore;
using YC4.Data;
using YC4.Entity;
using YC4.Interfaces;

namespace YC4.Services
{
    public class RoleService : IRoleInterface
    {
        private readonly ApplicationDbContext _context;
        public RoleService(ApplicationDbContext context) => _context = context;

        public async Task<List<Role>> GetAllAsync() => await _context.Roles.Include(r => r.RoleFunctions).ThenInclude(rf => rf.Function).ToListAsync();
        public async Task<Role?> GetByIdAsync(int roleId) => await _context.Roles.Include(r => r.RoleFunctions).ThenInclude(rf => rf.Function).FirstOrDefaultAsync(r => r.RoleId == roleId);
        public async Task<Role?> GetByNameAsync(string roleName) => await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
        public async Task<Role> CreateAsync(Role role) { _context.Roles.Add(role); await _context.SaveChangesAsync(); return role; }
        public async Task<Role> UpdateAsync(Role role) { _context.Roles.Update(role); await _context.SaveChangesAsync(); return role; }
        public async Task<bool> AssignFunctionAsync(int roleId, int functionId)
        {
            // 1. Kiểm tra xem Role và Function có tồn tại không
            var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == roleId);
            var funcExists = await _context.Functions.AnyAsync(f => f.FunctionId == functionId);
            if (!roleExists || !funcExists) return false;

            // 2. Kiểm tra xem quyền này đã được gán cho Role này chưa
            var exists = await _context.RoleFunctions
                .AnyAsync(rf => rf.RoleId == roleId && rf.FunctionId == functionId);

            if (exists) return false;

            // 3. Tiến hành lưu vào database
            _context.RoleFunctions.Add(new Role_Function
            {
                RoleId = roleId,
                FunctionId = functionId
            });

            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> DeleteAsync(int roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) return false;
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

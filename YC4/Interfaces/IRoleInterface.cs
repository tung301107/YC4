using YC4.Entity;

namespace YC4.Interfaces
{
    public interface IRoleInterface
    {
        Task<List<Role>> GetAllAsync();
        Task<Role?> GetByIdAsync(int roleId);
        Task<Role?> GetByNameAsync(string roleName);
        Task<Role> CreateAsync(Role role);
        Task<Role> UpdateAsync(Role role);
        Task<bool> DeleteAsync(int roleId);

        Task<bool> AssignFunctionAsync(int roleId, int functionId);
    }
}

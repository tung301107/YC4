using YC4.Entity;

namespace YC4.Interfaces
{
    public interface IFunctionInterface
    {
        Task<List<Function>> GetAllAsync();
        Task<Function?> GetByIdAsync(int functionId);
        Task<Function?> GetByCodeAsync(string functionCode);
        Task<Function> CreateAsync(Function function);
        Task<Function> UpdateAsync(Function function);
        Task<bool> DeleteAsync(int functionId);
    }
}

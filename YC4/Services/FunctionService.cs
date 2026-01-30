using Microsoft.EntityFrameworkCore;
using YC4.Data;
using YC4.Entity;
using YC4.Interfaces;

namespace YC4.Services
{
    public class FunctionService : IFunctionInterface
    {
        private readonly ApplicationDbContext _context;
        public FunctionService(ApplicationDbContext context) => _context = context;

        public async Task<List<Function>> GetAllAsync() => await _context.Functions.ToListAsync();
        public async Task<Function?> GetByIdAsync(int functionId) => await _context.Functions.FindAsync(functionId);
        public async Task<Function?> GetByCodeAsync(string functionCode) => await _context.Functions.FirstOrDefaultAsync(f => f.FunctionCode == functionCode);
        public async Task<Function> CreateAsync(Function function) { _context.Functions.Add(function); await _context.SaveChangesAsync(); return function; }
        public async Task<Function> UpdateAsync(Function function) { _context.Functions.Update(function); await _context.SaveChangesAsync(); return function; }
        public async Task<bool> DeleteAsync(int functionId)
        {
            var function = await _context.Functions.FindAsync(functionId);
            if (function == null) return false;
            _context.Functions.Remove(function);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

using YC4.Entity;

namespace YC4.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user, List<string> roles, List<string> permissions);
    }
}
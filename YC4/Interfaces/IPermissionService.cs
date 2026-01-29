namespace YC4.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> CheckUserPermissionAsync(int userId, string functionCode);
    }
}

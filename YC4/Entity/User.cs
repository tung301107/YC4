// File: YC4.Entity/User.cs
namespace YC4.Entity
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }      // Thêm mới
        public string? PhoneNumber { get; set; } // Thêm mới

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<UserFunction> UserFunctions { get; set; } = new List<UserFunction>();
    }
}
namespace YC4.Entity
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<User_Role> UserRoles { get; set; } = new List<User_Role>();
        public virtual ICollection<User_Function> UserFunctions { get; set; } = new List<User_Function>();
    }
}
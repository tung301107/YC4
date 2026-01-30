namespace YC4.Entity
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;

        public virtual ICollection<User_Role> UserRoles { get; set; } = new List<User_Role>();
        public virtual ICollection<Role_Function> RoleFunctions { get; set; } = new List<Role_Function>();
    }
}

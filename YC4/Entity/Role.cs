namespace YC4.Entity
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // Ví dụ: "Administrator"
        public string RoleCode { get; set; } = null!;// Ví dụ: "ADMIN"

        // Liên kết
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<RoleFunction> RoleFunctions { get; set; }
    }
}

namespace YC4.Entity
{
    public class Function
    {
        public int Id { get; set; }
        public string FunctionCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        // Liên kết
        public virtual ICollection<RoleFunction> RoleFunctions { get; set; }
        public virtual ICollection<UserFunction> UserFunctions { get; set; }
    }
}

namespace YC4.Entity
{
    public class Role_Function
    {
        public int RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;

        public int FunctionId { get; set; }
        public virtual Function Function { get; set; } = null!;
    }
}

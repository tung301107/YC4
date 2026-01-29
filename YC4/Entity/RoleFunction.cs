namespace YC4.Entity
{
    public class RoleFunction
    {
        public int RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;

        public int FunctionId { get; set; }
        public virtual Function Function { get; set; } = null!;
    }
}

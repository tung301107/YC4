namespace YC4.Entity
{
    public class User_Function
    {
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int FunctionId { get; set; }
        public virtual Function Function { get; set; } = null!;
    }
}

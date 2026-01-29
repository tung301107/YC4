namespace YC4.Entity
{
    public class UserFunction
    {
        public int UserId { get; set; }
        public virtual User User { get; set; } 

        public int FunctionId { get; set; }
        public virtual Function Function { get; set; }
    }
}

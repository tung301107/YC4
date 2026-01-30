namespace YC4.Entity
{
    public class Function
    {
        public int FunctionId { get; set; }
        public string FunctionCode { get; set; } = null!;
        public string FunctionName { get; set; } = null!;

        public virtual ICollection<User_Function> UserFunctions { get; set; } = new List<User_Function>();
        public virtual ICollection<Role_Function> RoleFunctions { get; set; } = new List<Role_Function>();
    }
}

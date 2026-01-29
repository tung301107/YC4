namespace YC4.DTOs
{
    public class UpdateProfileDto
    {
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}

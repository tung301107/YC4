namespace YC4.DTOs
{
    public class CreateEventDto
    {
        public string Name { get; set; } = null!;
        public DateTime DateTime { get; set; }
        public string Description { get; set; } = null!;
        public List<string> Rows { get; set; }
        public int SeatsPerRow { get; set; }
        public decimal Price { get; set; }
    }
}
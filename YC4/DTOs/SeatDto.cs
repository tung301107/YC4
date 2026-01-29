namespace YC4.DTOs
{
    public class SeatDto
    {
        public Guid SeatId { get; set; }
        public string RowName { get; set; }
        public int SeatNumber { get; set; }
        public bool IsAvailable { get; set; }
    }
}

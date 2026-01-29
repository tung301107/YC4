namespace YC4.DTOs
{
    public class BookingRequest
    {
        public Guid EventId { get; set; }
        public List<Guid> SelectedSeatIds { get; set; } = new();
    }
}

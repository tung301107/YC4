using System.ComponentModel.DataAnnotations.Schema;

namespace YC4.Entity
{
    public class Ticket
    {
        public Guid TicketId { get; set; }
        public Guid OrderId { get; set; }
        public Guid SeatId { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal PriceAtBooking { get; set; } // Lưu giá tại thời điểm mua
        public virtual Order Order { get; set; } = null!;
        public virtual Seat Seat { get; set; } = null!;

    }
}

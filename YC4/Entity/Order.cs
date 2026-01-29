using System.ComponentModel.DataAnnotations.Schema;

namespace YC4.Entity
{
    public class Order
    {
        public Guid OrderId { get; set; }
        // SỬA: Đổi từ Guid sang int để khớp với User.Id
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,0)")]
        public decimal TotalAmount { get; set; }
        public List<Ticket> Tickets { get; set; } = new();
    }
}
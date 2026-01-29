using System.ComponentModel.DataAnnotations; // Thêm cái này
using System.ComponentModel.DataAnnotations.Schema;

namespace YC4.Entity
{
    public class Seat
    {
        [Key] // Đánh dấu là khóa chính
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // <--- QUAN TRỌNG: Báo cho EF không tự sinh ID trong DB
        public Guid SeatId { get; set; }

        public string RowName { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
        public bool IsAvailable { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal Price { get; set; }
        public Guid EventId { get; set; }
    }
}
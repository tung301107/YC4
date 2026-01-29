using Microsoft.EntityFrameworkCore;
using YC4.Interfaces;
using YC4.Entity;
using YC4.DTOs;
using YC4.Data;

namespace YC4.Services
{
    public class EventService : IEventService
    {
        private readonly ApplicationDbContext _context;

        public EventService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateEventAsync(CreateEventDto dto)
        {
            var newEvent = new Event
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                DateTime = dto.DateTime,
                Description = dto.Description,
                TotalSeats = dto.Rows.Count * dto.SeatsPerRow
            };

            // Tự động tạo danh sách ghế dựa trên số hàng và số ghế mỗi hàng
            foreach (var rowName in dto.Rows)
            {
                for (int i = 1; i <= dto.SeatsPerRow; i++)
                {
                    newEvent.Seats.Add(new Seat
                    {
                        SeatId = Guid.NewGuid(),
                        RowName = rowName,
                        SeatNumber = i,
                        IsAvailable = true,
                        Price = dto.Price, // <-- Gán giá vé cho từng ghế
                        EventId = newEvent.Id
                    });
                }
            }

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return newEvent.Id;
        }

        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _context.Events
                .Include(e => e.Seats)
                .ToListAsync();
        }

        public async Task<bool> UpdateEventAsync(Guid id, CreateEventDto dto)
        {
            var existingEvent = await _context.Events.Include(e => e.Seats).FirstOrDefaultAsync(x => x.Id == id);
            if (existingEvent == null) return false;

            // 1. Cập nhật thông tin Event
            existingEvent.Name = dto.Name;
            existingEvent.DateTime = dto.DateTime;
            existingEvent.Description = dto.Description;
            existingEvent.TotalSeats = dto.Rows.Count * dto.SeatsPerRow;

            // 2. Xóa ghế cũ và lưu ngay để dọn Tracking
            if (existingEvent.Seats.Any())
            {
                _context.Seats.RemoveRange(existingEvent.Seats);
                await _context.SaveChangesAsync(); // Dọn dẹp database và bộ nhớ tracking
                existingEvent.Seats = new List<Seat>(); // Làm mới list trong memory
            }

            // 3. Thêm ghế mới
            foreach (var rowName in dto.Rows)
            {
                for (int i = 1; i <= dto.SeatsPerRow; i++)
                {
                    existingEvent.Seats.Add(new Seat
                    {
                        SeatId = Guid.NewGuid(), // Bạn tự tạo ID nên cần DatabaseGeneratedOption.None
                        RowName = rowName,
                        SeatNumber = i,
                        IsAvailable = true,
                        Price = dto.Price,
                        EventId = existingEvent.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
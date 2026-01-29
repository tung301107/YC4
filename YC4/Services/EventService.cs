using Microsoft.EntityFrameworkCore;
using YC4.Data;
using YC4.DTOs;
using YC4.Entity;
using YC4.Interfaces;

namespace YC4.Services
{
    public class EventService : IEventService
    {
        private readonly ApplicationDbContext _context;

        public EventService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Lấy danh sách sự kiện (Dùng cho trang chủ/quản lý)
        public async Task<IEnumerable<object>> GetAllEventsAsync()
        {
            return await _context.Events
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.DateTime,
                    e.Description,
                    TotalSeats = e.TotalSeats,
                    AvailableSeats = e.Seats.Count(s => s.IsAvailable)
                })
                .ToListAsync();
        }

        // 2. Lấy ghế trống (Dùng cho trang chọn chỗ)
        public async Task<IEnumerable<object>> GetAvailableSeatsAsync(Guid eventId)
        {
            return await _context.Seats
                .Where(s => s.EventId == eventId && s.IsAvailable)
                .OrderBy(s => s.RowName)
                .ThenBy(s => s.SeatNumber)
                .Select(s => new
                {
                    s.SeatId,
                    s.RowName,
                    s.SeatNumber,
                    s.Price
                })
                .ToListAsync();
        }

        // 3. Tạo sự kiện và tự động sinh ghế
        public async Task<Guid> CreateEventAsync(CreateEventDto dto)
        {
            var eventId = Guid.NewGuid();
            var newEvent = new Event
            {
                Id = eventId,
                Name = dto.Name,
                DateTime = dto.DateTime,
                Description = dto.Description,
                TotalSeats = dto.Rows.Count * dto.SeatsPerRow,
                Seats = new List<Seat>()
            };

            // Logic sinh ghế tự động: VD Rows=["A","B"], SeatsPerRow=10 => Sinh A1-A10, B1-B10
            foreach (var row in dto.Rows)
            {
                for (int i = 1; i <= dto.SeatsPerRow; i++)
                {
                    newEvent.Seats.Add(new Seat
                    {
                        SeatId = Guid.NewGuid(),
                        RowName = row,
                        SeatNumber = i,
                        IsAvailable = true,
                        Price = dto.Price,
                        EventId = eventId
                    });
                }
            }

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return eventId;
        }

        // 4. Cập nhật sự kiện (Có check ràng buộc đặt vé)
        public async Task<bool> UpdateEventAsync(Guid id, CreateEventDto dto)
        {
            var existingEvent = await _context.Events
                .Include(e => e.Seats)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existingEvent == null) return false;

            // KIỂM TRA: Nếu đã có người đặt vé cho bất kỳ ghế nào thuộc Event này thì KHÔNG cho đổi cấu hình ghế
            var isAnySeatBooked = await _context.Seats.AnyAsync(s => s.EventId == id && !s.IsAvailable);

            if (isAnySeatBooked)
            {
                // Nếu đã có người đặt, chỉ cho phép đổi thông tin text, không cho đổi Rows/SeatsPerRow/Price
                existingEvent.Name = dto.Name;
                existingEvent.DateTime = dto.DateTime;
                existingEvent.Description = dto.Description;
            }
            else
            {
                // Nếu chưa có ai đặt, cho phép làm mới toàn bộ cấu hình ghế
                existingEvent.Name = dto.Name;
                existingEvent.DateTime = dto.DateTime;
                existingEvent.Description = dto.Description;
                existingEvent.TotalSeats = dto.Rows.Count * dto.SeatsPerRow;

                // Xóa ghế cũ
                _context.Seats.RemoveRange(existingEvent.Seats);

                // Tạo lại ghế mới
                foreach (var row in dto.Rows)
                {
                    for (int i = 1; i <= dto.SeatsPerRow; i++)
                    {
                        _context.Seats.Add(new Seat
                        {
                            SeatId = Guid.NewGuid(),
                            RowName = row,
                            SeatNumber = i,
                            IsAvailable = true,
                            Price = dto.Price,
                            EventId = id
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // 5. Xóa sự kiện
        public async Task<bool> DeleteEventAsync(Guid id)
        {
            var ev = await _context.Events.Include(e => e.Seats).FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null) return false;

            // KIỂM TRA: Nếu có ghế nào đã được bán (IsAvailable = false), không cho xóa
            if (ev.Seats.Any(s => !s.IsAvailable))
                throw new Exception("Không thể xóa sự kiện đã có người mua vé!");

            _context.Events.Remove(ev);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
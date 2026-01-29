using Microsoft.EntityFrameworkCore;
using YC4.Entity;

namespace YC4.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Function> Functions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RoleFunction> RoleFunctions { get; set; }
        public DbSet<UserFunction> UserFunctions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình Many-to-Many
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<RoleFunction>().HasKey(rf => new { rf.RoleId, rf.FunctionId });
            modelBuilder.Entity<UserFunction>().HasKey(uf => new { uf.UserId, uf.FunctionId });

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // 1. Tạo các Function (Lưu ý: FunctionCode phải khớp với Claim "Permission" trong Program.cs)
            modelBuilder.Entity<Function>().HasData(
                new Function { Id = 1, FunctionCode = "CONCERT_view", Name = "Xem Concert" },
                new Function { Id = 2, FunctionCode = "CONCERT_CREATE", Name = "Thêm Concert" },
                new Function { Id = 3, FunctionCode = "Customer_MANAGEMENT", Name = "Quản lý Khách hàng" },
                new Function { Id = 4, FunctionCode = "CONCERT_UPDATE", Name = "Cập nhật sự kiện" },
                new Function { Id = 5, FunctionCode = "Available_Seat", Name = "Xem số ghế" },
                new Function { Id = 6, FunctionCode = "BOOK", Name = "Đặt vé" },
                new Function { Id = 7, FunctionCode = "ADMIN_MANAGE_USERS", Name = "Quản trị hệ thống" }
            );

            // 2. Tạo Role
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleCode = "ADMIN", Name = "Quản trị viên" },
                new Role { Id = 2, RoleCode = "CUSTOMER", Name = "Khách hàng" }
            );

            // 3. Gán quyền cho Role 
            // ADMIN có tất cả các quyền
            modelBuilder.Entity<RoleFunction>().HasData(
                new RoleFunction { RoleId = 1, FunctionId = 1 }, // CONCERT_view
                new RoleFunction { RoleId = 1, FunctionId = 2 }, // CONCERT_CREATE
                new RoleFunction { RoleId = 1, FunctionId = 3 }, // Customer_MANAGEMENT
                new RoleFunction { RoleId = 1, FunctionId = 4 }, // CONCERT_UPDATE
                new RoleFunction { RoleId = 1, FunctionId = 5 }, // Available_Seat
                new RoleFunction { RoleId = 1, FunctionId = 6 }, // BOOK
                new RoleFunction { RoleId = 1, FunctionId = 7 }, // ADMIN_MANAGE_USERS

                // CUSTOMER chỉ có quyền xem và đặt vé
                new RoleFunction { RoleId = 2, FunctionId = 1 }, // CONCERT_view
                new RoleFunction { RoleId = 2, FunctionId = 5 }, // Available_Seat
                new RoleFunction { RoleId = 2, FunctionId = 6 }  // BOOK
            );

            // 4. Tạo User (Mật khẩu nên được Hash trong thực tế)
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = "123", FullName = "Sếp Tổng" },
                new User { Id = 2, Username = "customer", Password = "123", FullName = "Nguyễn Văn A" }
            );

            // 5. Gán Role cho User
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { UserId = 1, RoleId = 1 },
                new UserRole { UserId = 2, RoleId = 2 }
            );
        }
    }
}
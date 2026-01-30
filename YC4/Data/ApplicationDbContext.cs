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

        // --- Định nghĩa các bảng ---
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

            // --- Cấu hình Mối quan hệ Many-to-Many (Bảng trung gian) ---

            // 1. UserRole: Nối User và Role
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId);
            });

            // 2. RoleFunction: Nối Role và Function (Quyền theo nhóm)
            modelBuilder.Entity<RoleFunction>(entity =>
            {
                entity.HasKey(rf => new { rf.RoleId, rf.FunctionId });

                entity.HasOne(rf => rf.Role)
                    .WithMany(r => r.RoleFunctions)
                    .HasForeignKey(rf => rf.RoleId);

                entity.HasOne(rf => rf.Function)
                    .WithMany(f => f.RoleFunctions)
                    .HasForeignKey(rf => rf.FunctionId);
            });

            // 3. UserFunction: Nối User và Function (Quyền đặc cách riêng lẻ)
            modelBuilder.Entity<UserFunction>(entity =>
            {
                entity.HasKey(uf => new { uf.UserId, uf.FunctionId });

                entity.HasOne(uf => uf.User)
                    .WithMany(u => u.UserFunctions)
                    .HasForeignKey(uf => uf.UserId);

                entity.HasOne(uf => uf.Function)
                    .WithMany(f => f.UserFunctions)
                    .HasForeignKey(uf => uf.FunctionId);
            });

            // --- Seed Data ---
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // 1. Khởi tạo Functions (Permission Codes)
            modelBuilder.Entity<Function>().HasData(
                new Function { Id = 1, FunctionCode = "CONCERT_view", Name = "Xem Concert" },
                new Function { Id = 2, FunctionCode = "CONCERT_CREATE", Name = "Thêm Concert" },
                new Function { Id = 3, FunctionCode = "Customer_MANAGEMENT", Name = "Quản lý Khách hàng" },
                new Function { Id = 4, FunctionCode = "CONCERT_UPDATE", Name = "Cập nhật sự kiện" },
                new Function { Id = 5, FunctionCode = "Available_Seat", Name = "Xem số ghế" },
                new Function { Id = 6, FunctionCode = "BOOK", Name = "Đặt vé" },
                new Function { Id = 7, FunctionCode = "ADMIN_MANAGE_USERS", Name = "Quản trị hệ thống" }
            );

            // 2. Khởi tạo Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleCode = "ADMIN", Name = "Quản trị viên" },
                new Role { Id = 2, RoleCode = "CUSTOMER", Name = "Khách hàng" }
            );

            // 3. Gán quyền mặc định cho Roles
            modelBuilder.Entity<RoleFunction>().HasData(
                // Admin full quyền
                new RoleFunction { RoleId = 1, FunctionId = 1 },
                new RoleFunction { RoleId = 1, FunctionId = 2 },
                new RoleFunction { RoleId = 1, FunctionId = 3 },
                new RoleFunction { RoleId = 1, FunctionId = 4 },
                new RoleFunction { RoleId = 1, FunctionId = 5 },
                new RoleFunction { RoleId = 1, FunctionId = 6 },
                new RoleFunction { RoleId = 1, FunctionId = 7 },
                // Customer quyền hạn chế
                new RoleFunction { RoleId = 2, FunctionId = 1 },
                new RoleFunction { RoleId = 2, FunctionId = 5 },
                new RoleFunction { RoleId = 2, FunctionId = 6 }
            );

            // 4. Khởi tạo Users mẫu
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = "123", FullName = "Sếp Tổng", Email = "admin@yc4.com" },
                new User { Id = 2, Username = "customer", Password = "123", FullName = "Nguyễn Văn A", Email = "nva@gmail.com" }
            );

            // 5. Gán Role cho Users
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { UserId = 1, RoleId = 1 },
                new UserRole { UserId = 2, RoleId = 2 }
            );
        }
    }
}
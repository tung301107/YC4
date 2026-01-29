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
        // Các thực thể chính
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Function> Functions { get; set; }

        // Các thực thể phụ (Bảng trung gian)
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RoleFunction> RoleFunctions { get; set; }
        public DbSet<UserFunction> UserFunctions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình bảng UserRole (Nhiều - Nhiều giữa User và Role)
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId }); // Khóa chính gồm 2 cột

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);


            // 2. Cấu hình bảng RoleFunction (Nhiều - Nhiều giữa Role và Function)
            modelBuilder.Entity<RoleFunction>()
                .HasKey(rf => new { rf.RoleId, rf.FunctionId });

            modelBuilder.Entity<RoleFunction>()
                .HasOne(rf => rf.Role)
                .WithMany(r => r.RoleFunctions)
                .HasForeignKey(rf => rf.RoleId);

            modelBuilder.Entity<RoleFunction>()
                .HasOne(rf => rf.Function)
                .WithMany(f => f.RoleFunctions)
                .HasForeignKey(rf => rf.FunctionId);


            // 3. Cấu hình bảng UserFunction (Nhiều - Nhiều giữa User và Function - Quyền đặc cách)
            modelBuilder.Entity<UserFunction>()
                .HasKey(uf => new { uf.UserId, uf.FunctionId });

            modelBuilder.Entity<UserFunction>()
                .HasOne(uf => uf.User)
                .WithMany(u => u.UserFunctions)
                .HasForeignKey(uf => uf.UserId);

            modelBuilder.Entity<UserFunction>()
                .HasOne(uf => uf.Function)
                .WithMany(f => f.UserFunctions)
                .HasForeignKey(uf => uf.FunctionId);

            // Seed dữ liệu mẫu (Tùy chọn nhưng nên có để test)
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // 1. Tạo các Function (Quyền)
            modelBuilder.Entity<Function>().HasData(
                new Function { Id = 1, FunctionCode = "CONCERT_VIEW", Name = "Xem Concert" },
                new Function { Id = 2, FunctionCode = "CONCERT_CREATE", Name = "Thêm Concert" },
                new Function { Id = 3, FunctionCode = "Customer_MANAGEMENT", Name = "Quản lý Khách hàng" },
                new Function { Id = 4, FunctionCode = "CONCERT_UPDATE", Name = "Cập nhật sự kiên" },
                new Function { Id = 5, FunctionCode = "Available_Seat", Name = "Xem số ghế", Description = "Xem số ghế chưa được đặt của 1 sự kiên" },
                new Function { Id = 6, FunctionCode = "BOOK", Name = "Đặt vé" }


            );

            // 2. Tạo Role (Vai trò)
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleCode = "ADMIN", Name = "Quản trị viên" },
                new Role { Id = 2, RoleCode = "Customer", Name = "Khách hàng" }
            );

            // 3. Gán quyền cho Role (Role_Function)
            modelBuilder.Entity<RoleFunction>().HasData(
                new RoleFunction { RoleId = 1, FunctionId = 1 }, 
                new RoleFunction { RoleId = 1, FunctionId = 2 }, 
                new RoleFunction { RoleId = 1, FunctionId = 3 }, 
                new RoleFunction { RoleId = 2, FunctionId = 1 },
                new RoleFunction { RoleId = 1, FunctionId = 4 },
                new RoleFunction { RoleId = 1, FunctionId = 5 },
                new RoleFunction { RoleId = 2, FunctionId = 5 },
                new RoleFunction { RoleId = 1, FunctionId = 6 },
                new RoleFunction { RoleId = 2, FunctionId = 6 } 

            );

            // 4. Tạo User
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = "123", FullName = "Sếp Tổng" },
                new User { Id = 2, Username = "Customer", Password = "123", FullName = "Khách hàng" }
            );

            // 5. Gán Role cho User (User_Role)
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { UserId = 1, RoleId = 1 }, // Gán admin vào role ADMIN
                new UserRole { UserId = 2, RoleId = 2 }  // Gán staff vào role STAFF
            );
        }
    }
}

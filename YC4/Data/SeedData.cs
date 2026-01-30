using Microsoft.EntityFrameworkCore;
using YC4.Entity;

namespace YC4.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.EnsureCreatedAsync();

            await SeedRolesAsync(context);
            await SeedFunctionsAsync(context);
            await SeedRoleFunctionsAsync(context);
            await SeedAdminUserAsync(context);
        }

        private static async Task SeedRolesAsync(ApplicationDbContext context)
        {
            if (await context.Roles.AnyAsync()) return;

            var roles = new List<Role>
            {
                new Role { RoleName = "Admin" },
                new Role { RoleName = "User" }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        private static async Task SeedFunctionsAsync(ApplicationDbContext context)
        {
            if (await context.Functions.AnyAsync()) return;

            var functions = new List<Function>
            {
                // User Management
                new Function { FunctionCode = "USER_VIEW", FunctionName = "Xem danh sách User" },
                new Function { FunctionCode = "USER_CREATE", FunctionName = "Tạo User" },
                new Function { FunctionCode = "USER_EDIT", FunctionName = "Sửa User" },
                new Function { FunctionCode = "USER_DELETE", FunctionName = "Xóa User" },
                new Function { FunctionCode = "USER_EXPORT", FunctionName = "Export User" },

                // Role Management
                new Function { FunctionCode = "ROLE_VIEW", FunctionName = "Xem danh sách Role" },
                new Function { FunctionCode = "ROLE_CREATE", FunctionName = "Tạo Role" },
                new Function { FunctionCode = "ROLE_EDIT", FunctionName = "Sửa Role" },
                new Function { FunctionCode = "ROLE_DELETE", FunctionName = "Xóa Role" },

                // Concert Management
                new Function { FunctionCode = "CONCERT_VIEW", FunctionName = "Xem Concert" },
                new Function { FunctionCode = "CONCERT_CREATE", FunctionName = "Thêm Concert" },
                new Function { FunctionCode = "CONCERT_UPDATE", FunctionName = "Cập nhật Concert" },
                
                // Booking
                new Function { FunctionCode = "BOOK", FunctionName = "Đặt vé" },
                new Function { FunctionCode = "SEAT_VIEW", FunctionName = "Xem số ghế" }
            };

            await context.Functions.AddRangeAsync(functions);
            await context.SaveChangesAsync();
        }

        private static async Task SeedRoleFunctionsAsync(ApplicationDbContext context)
        {
            if (await context.RoleFunctions.AnyAsync()) return;

            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");

            var allFunctions = await context.Functions.ToListAsync();
            var roleFunctions = new List<Role_Function>();

            if (adminRole != null)
            {
                foreach (var function in allFunctions)
                {
                    roleFunctions.Add(new Role_Function { RoleId = adminRole.RoleId, FunctionId = function.FunctionId });
                }
            }

            if (userRole != null)
            {
                var userFunctionCodes = new[] { "CONCERT_VIEW", "BOOK", "SEAT_VIEW" };
                foreach (var code in userFunctionCodes)
                {
                    var function = allFunctions.FirstOrDefault(f => f.FunctionCode == code);
                    if (function != null)
                        roleFunctions.Add(new Role_Function { RoleId = userRole.RoleId, FunctionId = function.FunctionId });
                }
            }

            await context.RoleFunctions.AddRangeAsync(roleFunctions);
            await context.SaveChangesAsync();
        }

        private static async Task SeedAdminUserAsync(ApplicationDbContext context)
        {
            if (await context.Users.AnyAsync(u => u.Username == "admin")) return;

            var passwordHash = BCrypt.Net.BCrypt.HashPassword("admin123");

            var adminUser = new User
            {
                Username = "admin",
                PasswordHash = passwordHash,
                FullName = "Administrator",
                Email = "admin@yc4.com",
                IsActive = true
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();

            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
            if (adminRole != null)
            {
                await context.UserRoles.AddAsync(new User_Role { UserId = adminUser.UserId, RoleId = adminRole.RoleId });
                await context.SaveChangesAsync();
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using YC4.Services;
using YC4.Data;
using YC4.Interfaces;
using YC4.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Đăng ký Distributed Cache (BẮT BUỘC để Session hoạt động)
builder.Services.AddDistributedMemoryCache();

// 3. Đăng ký Session với cấu hình chi tiết
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 4. Đăng ký các Service khác
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddTransient<IPriceCalculator, PriceCalculator>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// 5. Cấu hình Pipeline (Thứ tự rất quan trọng)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// LUÔN LUÔN đặt UseSession TRƯỚC UseAuthorization
app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();
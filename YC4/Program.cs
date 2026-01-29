using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using YC4.Data;
using YC4.Interfaces;
using YC4.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Cấu hình Authentication với JWT
var secretKey = "Chuoi_Key_Bi_Mat_Cua_Ban_Phai_Du_Dai_32_Ky_Tu";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// 3. Định nghĩa các Policy dựa trên Claim "Permission"
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewConcert", policy => policy.RequireClaim("Permission", "CONCERT_view"));
    options.AddPolicy("CanCreateConcert", policy => policy.RequireClaim("Permission", "CONCERT_CREATE"));
    options.AddPolicy("CanBookTicket", policy => policy.RequireClaim("Permission", "BOOK"));
    options.AddPolicy("CanManageUsers", policy => policy.RequireClaim("Permission", "ADMIN_MANAGE_USERS"));
    options.AddPolicy("CanViewAvailableSeats", policy => policy.RequireClaim("Permission", "Available_Seat"));
});

// 4. Đăng ký DI Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<IPriceCalculator, PriceCalculator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication(); // BẮT BUỘC: Xác thực trước
app.UseAuthorization();  // BẮT BUỘC: Phân quyền sau

app.MapControllers();
app.Run();
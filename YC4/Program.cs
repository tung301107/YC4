using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using YC4.Data;
using YC4.Entity;
using YC4.Interfaces;
using YC4.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Cấu hình Strongly Typed Settings (JwtSettings) ---
// Đọc cấu hình từ appsettings.json và đăng ký vào DI Container
var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);

var jwtSettings = jwtSection.Get<JwtSettings>();
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
{
    throw new Exception("JwtSettings is not configured properly in appsettings.json");
}
var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

// --- 2. Cấu hình DbContext ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 3. Cấu hình Authentication với JWT ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set true trong môi trường Production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Token hết hạn là vô hiệu lực ngay lập tức
    };
});

// --- 4. Cấu hình Authorization (Phân quyền) ---
builder.Services.AddAuthorization(options =>
{
    // Cấu hình Policy dựa trên Role
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));

    // Cấu hình Policy dựa trên Permission (FunctionCode)
    options.AddPolicy("CanViewConcert", policy => policy.RequireClaim("Permission", "CONCERT_view"));
    options.AddPolicy("CanCreateConcert", policy => policy.RequireClaim("Permission", "CONCERT_CREATE"));
    options.AddPolicy("CanUpdateConcert", policy => policy.RequireClaim("Permission", "CONCERT_UPDATE"));
    options.AddPolicy("CanViewAvailableSeats", policy => policy.RequireClaim("Permission", "Available_Seat"));
    options.AddPolicy("CanBookTicket", policy => policy.RequireClaim("Permission", "BOOK"));
    options.AddPolicy("CanManageUsers", policy => policy.RequireClaim("Permission", "ADMIN_MANAGE_USERS"));
    options.AddPolicy("CanManageCustomers", policy => policy.RequireClaim("Permission", "Customer_MANAGEMENT"));
});

// --- 5. Đăng ký DI Services & Swagger ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Cấu hình Swagger để hỗ trợ kiểm thử Token trực tiếp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "YC4 Ticketing API", Version = "v1" });

    // Cấu hình nút Authorize (Chiếc khóa) trên giao diện Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Vui lòng nhập Token theo định dạng: Bearer {your_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Đăng ký các Interface và Service thực thi
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<IPriceCalculator, PriceCalculator>();

// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// --- 6. Cấu hình Middleware Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "YC4 API v1"));
}

app.UseHttpsRedirection();

// QUAN TRỌNG: CORS phải đứng trước Authentication
app.UseCors("AllowAll");

// Thứ tự bắt buộc: Xác thực trước -> Phân quyền sau
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Console.WriteLine("=== YC4 TICKETING SYSTEM IS RUNNING ===");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
app.Run();
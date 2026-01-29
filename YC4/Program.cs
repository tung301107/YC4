using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using YC4.Data;
using YC4.Interfaces;
using YC4.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Cấu hình DbContext ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 2. Cấu hình Authentication với JWT ---
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

// --- 3. Cấu hình Authorization (Phân quyền) ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewConcert", policy => policy.RequireClaim("Permission", "CONCERT_view"));
    options.AddPolicy("CanCreateConcert", policy => policy.RequireClaim("Permission", "CONCERT_CREATE"));
    options.AddPolicy("CanUpdateConcert", policy => policy.RequireClaim("Permission", "CONCERT_UPDATE"));
    options.AddPolicy("CanViewAvailableSeats", policy => policy.RequireClaim("Permission", "Available_Seat"));
    options.AddPolicy("CanBookTicket", policy => policy.RequireClaim("Permission", "BOOK"));
    options.AddPolicy("CanManageUsers", policy => policy.RequireClaim("Permission", "ADMIN_MANAGE_USERS"));
    options.AddPolicy("CanManageCustomers", policy => policy.RequireClaim("Permission", "Customer_MANAGEMENT"));
});

// --- 4. Đăng ký DI Services & Swagger ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Cấu hình Swagger đầy đủ để hiện nút Authorize (Chiếc khóa)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "YC4 API", Version = "v1" });

    // Định nghĩa Schema Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập Token theo cú pháp: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Áp dụng Security Requirement
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<IPriceCalculator, PriceCalculator>();

var app = builder.Build();

// --- 5. Cấu hình Middleware ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Lưu ý: Authentication PHẢI nằm TRƯỚC Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
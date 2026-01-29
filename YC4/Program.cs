using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // Thêm cái này
using System.Text;
using YC4.Data;
using YC4.Interfaces;
using YC4.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

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
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

// 3. Cấu hình Policy-based Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("ADMIN"));
    options.AddPolicy("CanViewConcert", policy => policy.RequireClaim("Permission", "CONCERT_VIEW", "CONCERT_view"));
    options.AddPolicy("CanCreateConcert", policy => policy.RequireClaim("Permission", "CONCERT_CREATE"));
    options.AddPolicy("CanUpdateConcert", policy => policy.RequireClaim("Permission", "CONCERT_UPDATE"));
    options.AddPolicy("CanBookTicket", policy => policy.RequireClaim("Permission", "BOOK"));
    options.AddPolicy("CanViewAvailableSeats", policy => policy.RequireClaim("Permission", "Available_Seat"));
    options.AddPolicy("CanManageUsers", policy => policy.RequireClaim("Permission", "ADMIN_MANAGE_USERS"));
});

// 4. Đăng ký các Service (Bổ sung IAccountService)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Cấu hình Swagger để hỗ trợ Bearer Token
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "YC4 API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập Token theo cú pháp: Bearer {your_token}",
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
            new string[] {}
        }
    });
});

builder.Services.AddHttpContextAccessor();

// DI Services - QUAN TRỌNG: Thêm AccountService ở đây
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddTransient<IPriceCalculator, PriceCalculator>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// 5. Cấu hình Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
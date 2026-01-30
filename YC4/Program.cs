using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using YC4.Data;
using YC4.Interfaces;
using YC4.Services;
using YC4.Entity;

var builder = WebApplication.CreateBuilder(args);

// --- JWT Settings ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

// --- DB Context ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Services DI ---
builder.Services.AddScoped<IUserInterface, UserService>();
builder.Services.AddScoped<IRoleInterface, RoleService>();
builder.Services.AddScoped<IFunctionInterface, FunctionService>();
builder.Services.AddScoped<IAuthInterface, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPriceCalculator, PriceCalculator>();

// --- Authentication ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var secretKey = jwtSettings["SecretKey"];
    var key = Encoding.UTF8.GetBytes(secretKey!);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// --- Authorization ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    
    // User Management
    options.AddPolicy("CanViewUsers", policy => policy.RequireClaim("Permission", "USER_VIEW"));
    options.AddPolicy("CanCreateUsers", policy => policy.RequireClaim("Permission", "USER_CREATE"));
    options.AddPolicy("CanEditUsers", policy => policy.RequireClaim("Permission", "USER_EDIT"));
    options.AddPolicy("CanDeleteUsers", policy => policy.RequireClaim("Permission", "USER_DELETE"));
    
    // Role Management
    options.AddPolicy("CanManageRoles", policy => policy.RequireClaim("Permission", "ROLE_VIEW", "ROLE_CREATE", "ROLE_EDIT", "ROLE_DELETE"));
    
    // Concert Management
    options.AddPolicy("CanViewConcert", policy => policy.RequireClaim("Permission", "CONCERT_VIEW"));
    options.AddPolicy("CanCreateConcert", policy => policy.RequireClaim("Permission", "CONCERT_CREATE"));
    options.AddPolicy("CanUpdateConcert", policy => policy.RequireClaim("Permission", "CONCERT_UPDATE"));
    
    // Booking & Seats
    options.AddPolicy("CanBookTicket", policy => policy.RequireClaim("Permission", "BOOK"));
    options.AddPolicy("CanViewSeats", policy => policy.RequireClaim("Permission", "SEAT_VIEW"));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập Token của bạn theo định dạng: Bearer {token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalDev", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// --- Seed Data ---
try
{
    await SeedData.InitializeAsync(app.Services);
}
catch (Exception ex)
{
    Console.WriteLine($"Error during database initialization: {ex.Message}");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AllowLocalDev");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
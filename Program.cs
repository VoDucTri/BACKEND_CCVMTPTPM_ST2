using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using nhom5_webAPI.Models;
using nhom5_webAPI.Repositories;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// **Cấu hình cơ sở dữ liệu**: Dùng SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// **Đăng ký Identity**: Quản lý User và Role
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// **Đăng ký Repositories**: Dependency Injection cho các repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();


// **Thêm dịch vụ Controllers**: Xử lý JSON và ngăn lỗi tuần hoàn dữ liệu
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// **Thêm Swagger**: Hỗ trợ tài liệu hóa API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    // Cấu hình nút Authorize
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGci...\""
    });

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
            Array.Empty<string>()
        }
    });
});

// **Cấu hình CORS**: Cho phép mọi nguồn gốc (tạm thời trong phát triển)
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyAllowOrigins", policy =>
    {
        policy.AllowAnyOrigin() // Cho phép tất cả nguồn gốc
              .AllowAnyHeader() // Cho phép tất cả các header
              .AllowAnyMethod(); // Cho phép tất cả các phương thức
    });
});

builder.Services.AddAuthorization(options =>
{
    // Thêm chính sách phân quyền cho tất cả các quyền
    var permissions = new string[] { "product", "pet", "appointment", "order", "service", "role" , "user" };
    foreach (var permission in permissions)
    {
        options.AddPolicy($"{permission}.View", policy => policy.RequireClaim("permission", $"{permission}.view"));
        options.AddPolicy($"{permission}.Create", policy => policy.RequireClaim("permission", $"{permission}.create"));
        options.AddPolicy($"{permission}.Edit", policy => policy.RequireClaim("permission", $"{permission}.edit"));
        options.AddPolicy($"{permission}.Delete", policy => policy.RequireClaim("permission", $"{permission}.delete"));
        options.AddPolicy($"{permission}.All", policy => policy.RequireClaim("permission", $"{permission}.all"));
    }
});


// **Cấu hình JWT Authentication**
var jwtSettings = builder.Configuration.GetSection("JWTKey");
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
.AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["ValidIssuer"],
        ValidAudience = jwtSettings["ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        RoleClaimType = ClaimTypes.Role
    };
});

// **Thêm Authorization**
builder.Services.AddAuthorization();
builder.Services.AddScoped<IClaimsTransformation, nhom5_webAPI.Services.DynamicClaimTransformer>();
var app = builder.Build();

// **Tạo tài khoản Admin mặc định**
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Tạo vai trò Admin nếu chưa tồn tại
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Tạo tài khoản Admin mặc định nếu chưa có
    var adminUser = await userManager.FindByNameAsync("admin");
    if (adminUser == null)
    {
        var user = new User
        {
            UserName = "admin",
            Email = "admin@example.com",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");

            // Thêm claim permission:user.view vào role Admin
            var adminRole = await roleManager.FindByNameAsync("Admin");
            var existingClaims = await roleManager.GetClaimsAsync(adminRole);
            if (!existingClaims.Any(c => c.Type == "permission" && c.Value == "user.view"))
            {
                await roleManager.AddClaimAsync(adminRole, new Claim("permission", "user.view"));
            }
        }
    }
}

    // **Cấu hình Middleware**
    if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error"); // Endpoint xử lý lỗi tùy chỉnh
    app.UseHsts(); // Enforce HTTPS trong môi trường production
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("MyAllowOrigins"); // Cấu hình CORS
app.UseAuthentication();       // Xác thực
app.UseAuthorization();        // Phân quyền

app.MapControllers();

app.Run();
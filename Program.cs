using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using CarDealershipAPI.Data;
using CarDealershipAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Car Dealership API",
        Version = "v1",
        Description = "A comprehensive car dealership management system with role-based access and OTP security"
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Configure Database (In-Memory for this demo)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("CarDealershipDb"));

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyForJWTTokenGeneration12345";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CarDealershipAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CarDealershipAPI";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// Register services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IOtpService, OtpService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed database with sample data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var passwordService = services.GetRequiredService<IPasswordService>();
    
    await SeedData(context, passwordService);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Seed data method
async Task SeedData(ApplicationDbContext context, IPasswordService passwordService)
{
    // Check if data already exists
    if (await context.Users.AnyAsync())
    {
        return;
    }

    // Create admin user
    var admin = new CarDealershipAPI.Models.User
    {
        Username = "admin",
        Email = "admin@cardealership.com",
        PasswordHash = passwordService.HashPassword("Admin@123"),
        Role = "Admin",
        CreatedAt = DateTime.UtcNow
    };
    context.Users.Add(admin);

    // Create sample customer
    var customer = new CarDealershipAPI.Models.User
    {
        Username = "customer1",
        Email = "customer@example.com",
        PasswordHash = passwordService.HashPassword("Customer@123"),
        Role = "Customer",
        CreatedAt = DateTime.UtcNow
    };
    context.Users.Add(customer);

    // Create sample vehicles
    var vehicles = new[]
    {
        new CarDealershipAPI.Models.Vehicle
        {
            Make = "Toyota",
            Model = "Camry",
            Year = 2023,
            Price = 28500.00m,
            Color = "Silver",
            Mileage = 15000,
            VIN = "1HGBH41JXMN109186",
            Description = "Excellent condition, one owner, full service history",
            IsAvailable = true
        },
        new CarDealershipAPI.Models.Vehicle
        {
            Make = "Honda",
            Model = "Accord",
            Year = 2022,
            Price = 26800.00m,
            Color = "Black",
            Mileage = 22000,
            VIN = "2HGBH41JXMN109187",
            Description = "Well maintained, leather interior, sunroof",
            IsAvailable = true
        },
        new CarDealershipAPI.Models.Vehicle
        {
            Make = "Ford",
            Model = "F-150",
            Year = 2024,
            Price = 45000.00m,
            Color = "Blue",
            Mileage = 5000,
            VIN = "3HGBH41JXMN109188",
            Description = "Brand new condition, 4WD, extended cab",
            IsAvailable = true
        },
        new CarDealershipAPI.Models.Vehicle
        {
            Make = "Tesla",
            Model = "Model 3",
            Year = 2023,
            Price = 42000.00m,
            Color = "White",
            Mileage = 8000,
            VIN = "4HGBH41JXMN109189",
            Description = "Electric, autopilot, premium interior",
            IsAvailable = true
        },
        new CarDealershipAPI.Models.Vehicle
        {
            Make = "BMW",
            Model = "X5",
            Year = 2022,
            Price = 58000.00m,
            Color = "Gray",
            Mileage = 18000,
            VIN = "5HGBH41JXMN109190",
            Description = "Luxury SUV, all-wheel drive, premium package",
            IsAvailable = true
        },
        new CarDealershipAPI.Models.Vehicle
        {
            Make = "Mercedes-Benz",
            Model = "C-Class",
            Year = 2023,
            Price = 48500.00m,
            Color = "Black",
            Mileage = 12000,
            VIN = "6HGBH41JXMN109191",
            Description = "Elegant sedan, advanced safety features",
            IsAvailable = true
        },
        new CarDealershipAPI.Models.Vehicle
        {
            Make = "Chevrolet",
            Model = "Silverado",
            Year = 2023,
            Price = 42500.00m,
            Color = "Red",
            Mileage = 10000,
            VIN = "7HGBH41JXMN109192",
            Description = "Powerful truck, towing package included",
            IsAvailable = true
        },
        new CarDealershipAPI.Models.Vehicle
        {
            Make = "Nissan",
            Model = "Altima",
            Year = 2022,
            Price = 24500.00m,
            Color = "White",
            Mileage = 28000,
            VIN = "8HGBH41JXMN109193",
            Description = "Reliable sedan, fuel efficient",
            IsAvailable = true
        },
        new CarDealershipAPI.Models.Vehicle
        {
            Make = "Hyundai",
            Model = "Tucson",
            Year = 2023,
            Price = 32000.00m,
            Color = "Blue",
            Mileage = 14000,
            VIN = "9HGBH41JXMN109194",
            Description = "Compact SUV, great for families",
            IsAvailable = true
        },
        new CarDealershipAPI.Models.Vehicle
        {
            Make = "Mazda",
            Model = "CX-5",
            Year = 2024,
            Price = 35500.00m,
            Color = "Red",
            Mileage = 3000,
            VIN = "AHGBH41JXMN109195",
            Description = "Nearly new, sporty design, excellent handling",
            IsAvailable = true
        }
    };

    context.Vehicles.AddRange(vehicles);
    await context.SaveChangesAsync();

    Console.WriteLine("Database seeded successfully!");
    Console.WriteLine("Admin credentials: admin@cardealership.com / Admin@123");
    Console.WriteLine("Customer credentials: customer@example.com / Customer@123");
}

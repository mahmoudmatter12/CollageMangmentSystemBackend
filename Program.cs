using CollageMangmentSystem.Core.Interfaces;
using CollageMangmentSystem.Infrastructure.Data;
using CollageMangmentSystem.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using CollageMangmentSystem.Infrastructure.Services;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
// Add to your service collection
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
// Register ApplicationDbContext with PostgreSQL provider
// Connection string is read from appsettings.json under "Postgres" key
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Register generic repository pattern (scoped lifetime)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddRateLimiter(options =>
{
    // Policy for general GET requests (strict limits)
    options.AddFixedWindowLimiter("FixedWindowPolicy", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10); // 10-second time window
        opt.PermitLimit = 5;                  // Max 5 requests per window
        opt.QueueLimit = 0;                   // No queuing - immediate rejection
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // Stricter policy for POST/PUT/DELETE operations
    options.AddFixedWindowLimiter("StrictPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(10); // 10-minute window  
        opt.PermitLimit = 10;                  // Max 10 requests per window
        opt.QueueLimit = 0;                    // Immediate rejection
    });

    // Custom handler when rate limit is exceeded
    options.OnRejected = (context, _) =>
    {
        // Add Retry-After header (seconds remaining in window)
        context.HttpContext.Response.Headers["Retry-After"] = 
            (600 - DateTime.Now.Second % 600).ToString(); // 10 minutes in seconds
        return new ValueTask();
    };

    // Set HTTP 429 status code for rate-limited requests
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});


var app = builder.Build();

// Enable Swagger JSON endpoint
app.UseSwagger();

// Enable Swagger UI (interactive documentation)
app.UseSwaggerUI();

// Activate rate limiting middleware
// MUST be placed before MapControllers()
app.UseRateLimiter();

// Map controller endpoints
// Note: We don't apply global rate limiting here (use attributes per controller)
app.MapControllers();

// Start the application
app.Run();
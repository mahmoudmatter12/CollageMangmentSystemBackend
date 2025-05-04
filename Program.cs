using CollageMangmentSystem.Core.Interfaces;
using CollageMangmentSystem.Infrastructure.Data;
using CollageMangmentSystem.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using CollageManagementSystem.Services.Auth;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using CollageManagementSystem.Services;
using CollageManagementSystem.Middleware;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Authentication.Cookies;
using CollageMangmentSystem.Core.Entities.course;
using CollageManagementSystem.Core.Entities.userEnrollments;
using CollageMangmentSystem.Infrastructure.Middlewares;
using CollageManagementSystem.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
// Add infrastructure services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.AddTransient<AuthorizationMiddleware>(provider =>
{
    var next = provider.GetRequiredService<RequestDelegate>();
    return new AuthorizationMiddleware(next);
});
// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped(typeof(IDepRepostaory<>), typeof(DepRepostaory<>));
builder.Services.AddScoped(typeof(ICourseReposatory<>), typeof(CourseReposatory<>));
builder.Services.AddScoped(typeof(IUserEnrollments<>), typeof(UserEnrollmentsRepostaory<>));
builder.Services.AddScoped<IAdminReposatory, AdminReposatory>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();

// Configure cookie policy
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// Configure authentication cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "CollageAuth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.LoginPath = "/api/auth/login";
    options.AccessDeniedPath = "/api/auth/access-denied";
});

// Configure rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("FixedWindowPolicy", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    options.AddFixedWindowLimiter("StrictPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(10);
        opt.PermitLimit = 10;
        opt.QueueLimit = 0;
    });

    options.OnRejected = (context, _) =>
    {
        context.HttpContext.Response.Headers["Retry-After"] =
            (600 - DateTime.Now.Second % 600).ToString();
        return new ValueTask();
    };

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Configure JWT authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("SecretKey is not configured");
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["accessToken"];
            return Task.CompletedTask;
        }
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);


// Register application services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/swagger"))
    {
        await next(context);
        return;
    }

    await next(context); // Proceed to the next middleware
});

app.UseHttpsRedirection();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.None,
    Secure = CookieSecurePolicy.SameAsRequest,
    HttpOnly = HttpOnlyPolicy.Always
});

app.UseMiddleware<GlobalExceptionMiddleware>();
// Use middleware with factory approach
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/auth") || context.Request.Path.StartsWithSegments("/swagger"))
    {
        await next(context);
        return;
    }
    var tokenService = context.RequestServices.GetRequiredService<ITokenService>();
    var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
    var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();

    var middleware = new JwtAuthenticationMiddleware(next, configuration, tokenService, env);
    await middleware.Invoke(context);
});
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuthorizationMiddleware>(); // Role-based authorization
app.MapControllers();

app.Run();
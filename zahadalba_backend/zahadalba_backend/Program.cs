using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using zahadalba_backend.Configurations;
using zahadalba_backend.Data;
using zahadalba_backend.Data.Entities;
using zahadalba_backend.Features.Auth;
using zahadalba_backend.Features.Auth.GetMe;
using zahadalba_backend.Features.Auth.Refresh;
using zahadalba_backend.Features.Auth.SignIn;
using zahadalba_backend.Features.Auth.SignOut;
using zahadalba_backend.Features.Auth.SignUp;
using zahadalba_backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings is null)
{
    throw new InvalidOperationException("JwtSettings section is missing in configuration");
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddHostedService<ExpiredTokensCleanerService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("signInLimit", httpContext =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
            factory: _ =>
            {
                return new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 5,
                };
            });
    });
});

builder.Services.AddValidatorsFromAssemblyContaining<SignUpRequestDtoValidator>();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IDateTimeService, DateTimeService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ISha256HasherService, Sha256HasherService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Users.Any(u => u.Email == "admin@a.aa"))
    {
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        User testUser = new User()
        {
            FirstName = "Admin",
            LastName = "Admin",
            Age = 33,
            Email = "admin@a.aa",
            Phone = "88888888888"
        };
        testUser.PasswordHash = passwordHasher.HashPassword(testUser, "a");

        db.Users.Add(testUser);
        await db.SaveChangesAsync();
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapSignUpEndpoint();
app.MapSignInEndpoint();
app.MapGetMeEndpoint();
app.MapRefreshEndpoint();
app.MapSignOutEndpoint();

app.Run("http://localhost:7203");

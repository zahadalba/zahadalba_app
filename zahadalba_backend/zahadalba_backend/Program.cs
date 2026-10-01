using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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

builder.Services.AddSingleton<AppDbContext>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IDateTimeService, DateTimeService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ISha256HasherService, Sha256HasherService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var store = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!store.Users.Any(u => u.Email == "1"))
    {
        var passwordHasher = scope.ServiceProvider.GetRequiredService<ISha256HasherService>();

        User testUser = new User()
        {
            Email = "1",
            PasswordHash = passwordHasher.GetHash("1")
        };

        store.Users.Add(testUser);
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.MapSignUpEndpoint();
app.MapSignInEndpoint();
app.MapGetMeEndpoint();
app.MapRefreshEndpoint();
app.MapSignOutEndpoint();

app.Run("http://localhost:7203");

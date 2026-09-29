using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var usersDb = new List<User> { new User { Email = "1", Password = "1" } };

var refreshTokensDb = new List<RefreshToken>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = AuthOptions.ISSUER,
            ValidateAudience = true,
            ValidAudience = AuthOptions.AUDIENCE,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/sign-in", (SignInRequestDto dto) =>
{
    var user = usersDb.FirstOrDefault(u => u.Email == dto.Email && u.Password == dto.Password);
    if (user is null)
    {
        return Results.Problem(statusCode: 409, detail: "Неверный email или пароль");
    }

    var claims = new List<Claim>
    { 
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
    };

    var jwtToken = new JwtSecurityToken(
        issuer: AuthOptions.ISSUER,
        audience: AuthOptions.AUDIENCE,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(2),
        signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

    var jwtTokenHandler = new JwtSecurityTokenHandler();
    var accessToken = jwtTokenHandler.WriteToken(jwtToken);

    var refreshToken = GenerateRefreshToken();
    refreshTokensDb.Add(new RefreshToken { Token = refreshToken, UserId = user.Id });

    return Results.Ok(new { Id = user.Id, Email = user.Email, accessToken, refreshToken });
});

app.MapGet("/api/auth/me", (HttpContext context) =>
{
    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId is null)
    {
        return Results.Problem(statusCode: 401, title: "Unauthorized" );
    }

    var user = usersDb.FirstOrDefault(u => u.Id.ToString() == userId);
    if (user is null)
    {
        return Results.Problem(statusCode: 401, title: "Unauthorized");
    }

    return Results.Ok(new { Id = user.Id.ToString(), user.Email });
}).RequireAuthorization();

app.MapPost("/api/auth/refresh", (HttpContext context) =>
{
    Console.WriteLine("refresh");
    var refreshTokenData = context.Request.Cookies["refreshToken"];
    if (refreshTokenData is null)
    {
        return Results.Problem(statusCode: 401, title: "Unauthorized");
    }

    var existingRefreshToken = refreshTokensDb.FirstOrDefault(t => t.Token == refreshTokenData);
    if (existingRefreshToken is null || existingRefreshToken.ExpiresAt <= DateTime.UtcNow)
    {
        return Results.Problem(statusCode: 401, title: "Unauthorized");
    }

    // --- GRACE PERIOD ДЛЯ ЗАЩИТЫ ОТ ГОНКИ (PARALLEL REQUESTS) ---
    if (existingRefreshToken.IsRevoked)
    {
        // Если токен уже отозван, проверяем: а не прошло ли меньше 10 секунд?
        var timeSinceRevocation = DateTime.UtcNow - existingRefreshToken.RevokedAt;

        if (timeSinceRevocation.TotalSeconds <= 10)
        {
            // Это параллельный запрос от того же клиента (например, F5 / несколько запросов сразу).
            // Выдаем новые токены повторно, не ломая клиента!
            var retryUser = usersDb.FirstOrDefault(u => u.Id == existingRefreshToken.UserId);
            if (retryUser is not null)
            {
                var retryClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, retryUser.Id.ToString()),
                    new Claim(ClaimTypes.Email, retryUser.Email),
                };

                var retryJwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: retryClaims,
                    expires: DateTime.UtcNow.AddMinutes(2),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

                var retryAccessToken = new JwtSecurityTokenHandler().WriteToken(retryJwt);
                var retryRefreshToken = GenerateRefreshToken();

                refreshTokensDb.Add(new RefreshToken { Token = retryRefreshToken, UserId = retryUser.Id });

                return Results.Ok(new { accessToken = retryAccessToken, refreshToken = retryRefreshToken });
            }
        }

        // Если прошло больше 10 секунд — это реальная попытка повторного использования старого токена
        return Results.Problem(statusCode: 401, title: "Unauthorized");
    }
    // -------------------------------------------------------------

    var user = usersDb.FirstOrDefault(u => u.Id == existingRefreshToken.UserId);
    if (user is null)
    {
        return Results.Problem(statusCode: 401, title: "Unauthorized");
    }

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
    };

    var jwtToken = new JwtSecurityToken(
        issuer: AuthOptions.ISSUER,
        audience: AuthOptions.AUDIENCE,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(2),
        signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

    var jwtTokenHandler = new JwtSecurityTokenHandler();
    var accessToken = jwtTokenHandler.WriteToken(jwtToken);

    var refreshToken = GenerateRefreshToken();
    refreshTokensDb.Add(new RefreshToken { Token = refreshToken, UserId = user.Id });

    // Помечаем текущий как отозванный и сохраняем время отзыва
    existingRefreshToken.IsRevoked = true;
    existingRefreshToken.RevokedAt = DateTime.UtcNow;

    foreach (var t in refreshTokensDb)
    {
        Console.WriteLine($"{t.CreatedAt.ToString()} | {t.RevokedAt.ToString()} | {t.IsRevoked}");
    }

    return Results.Ok(new { accessToken, refreshToken });
});
app.MapPost("/api/auth/sign-out", (HttpContext context) =>
{
    var refreshTokenValue = context.Request.Cookies["refreshToken"];

    if (!string.IsNullOrEmpty(refreshTokenValue))
    {
        var existingToken = refreshTokensDb.FirstOrDefault(t => t.Token == refreshTokenValue);
        if (existingToken is not null)
        {
            existingToken.IsRevoked = true;
            existingToken.RevokedAt = DateTime.UtcNow;
        }
    }

    return Results.Ok(new { success = true });
});

string GenerateRefreshToken()
{
    var bytes = RandomNumberGenerator.GetBytes(64);
    return Convert.ToBase64String(bytes);
}

app.Run("http://localhost:7203");

public class AuthOptions
{
    public const string ISSUER = "MyAuthServer";
    public const string AUDIENCE = "MyAuthClient";
    const string KEY = "mysupersecretdgdgdgsdgsdgsfhfhdgjjjjjjjjjggggggg_key24";
    public static SymmetricSecurityKey GetSymmetricSecurityKey() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}

public record SignInRequestDto(string Email, string Password);

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = "";
    public Guid UserId { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(30);
}
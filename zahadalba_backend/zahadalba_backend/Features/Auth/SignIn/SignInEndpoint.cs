using zahadalba_backend.Data;
using zahadalba_backend.Data.Entities;
using zahadalba_backend.Services;

namespace zahadalba_backend.Features.Auth.SignIn
{
    public static class SignInEndpoint
    {
        public static void MapSignInEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/sign-in", Handle);
        }

        private static IResult Handle(
            SignInRequestDto dto,
            IDateTimeService dateTimeService,
            AppDbContext dbContext,
            ITokenService tokenService,
            ISha256HasherService sha256HasherService)
        {
            var user = dbContext.Users.FirstOrDefault(u => u.Email == dto.Email && u.PasswordHash == sha256HasherService.GetHash(dto.Password));
            if (user is null)
            {
                return Results.Problem(statusCode: 409, detail: "Неверный email или пароль");
            }

            var accessToken = tokenService.GenerateAccessToken(user, dateTimeService);
            var refreshToken = tokenService.GenerateRefreshToken();

            dbContext.RefreshTokens.Add(new RefreshToken(
                userId: user.Id,
                tokenHash: sha256HasherService.GetHash(refreshToken),
                createdAt: dateTimeService.UtcNow,
                expiresAt: dateTimeService.UtcNow.AddDays(7),
                absoluteExpiresAt: dateTimeService.UtcNow.AddDays(30)));

            return Results.Ok(new
            {
                Id = user.Id.ToString(),
                user.Email,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }
    }
}

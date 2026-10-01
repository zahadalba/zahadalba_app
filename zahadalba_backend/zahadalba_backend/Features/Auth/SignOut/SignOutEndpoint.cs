using zahadalba_backend.Data;
using zahadalba_backend.Services;

namespace zahadalba_backend.Features.Auth.SignOut
{
    public static class SignOutEndpoint
    {
        public static void MapSignOutEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/sign-out", Handler);
        }

        private static IResult Handler(
            HttpContext context,
            IDateTimeService dateTimeService,
            AppDbContext dbContext,
            ISha256HasherService sha256HasherService)
        {
            var refreshTokenData = context.Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshTokenData))
            {
                var existingRefreshToken = dbContext.RefreshTokens.FirstOrDefault(t => t.TokenHash == sha256HasherService.GetHash(refreshTokenData));
                if (existingRefreshToken is not null)
                {
                    existingRefreshToken.IsRevoked = true;
                    existingRefreshToken.RevokedAt = dateTimeService.UtcNow;
                }
            }
            return Results.Ok();
        }
    }
}
